using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Exceptions;

namespace northguan_nsa_vue_app.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasetoService _pasetoService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IPasetoService pasetoService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _pasetoService = pasetoService;
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            // Find user by username or email
            var user = await _userManager.FindByNameAsync(username) ??
                      await _userManager.FindByEmailAsync(username);

            if (user == null)
            {
                throw new AuthenticationException("帳號或密碼錯誤");
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    throw new AuthenticationException(ErrorCodes.ACCOUNT_LOCKED, "帳號已被鎖定，請稍後再試");
                }
                throw new AuthenticationException("帳號或密碼錯誤");
            }

            // Generate PASETO token
            var token = await GeneratePasetoTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            // Store user info in HTTP context for this request
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("username", user.UserName ?? ""),
                new Claim("isReadOnly", user.ReadOnly.ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "password");
            var principal = new ClaimsPrincipal(identity);
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.User = principal;
            }

            return new LoginResponse
            {
                Token = token,
                User = new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Username = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "User",
                    Phone = user.PhoneNumber,
                    EmployeeId = user.EmployeeId,
                    IsReadOnly = user.ReadOnly
                }
            };
        }

        public async Task<string> GeneratePasetoTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return _pasetoService.GenerateToken(user, roles);
        }

        public string GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }
            throw new AuthorizationException("無法取得當前使用者ID");
        }

        // Get current user's role
        public string GetCurrentUserRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            if (roleClaim != null)
            {
                return roleClaim.Value;
            }
            throw new AuthorizationException("無法取得當前使用者角色");
        }

        public bool IsAdmin()
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
        }

        public List<int> GetAvailableStationIds()
        {
            if (IsAdmin())
            {
                // Admin can access all stations
                return _context.Stations
                    .Where(s => s.DeletedAt == null)
                    .Select(s => s.Id)
                    .ToList();
            }

            // For non-admin users, get stations from permissions
            var userId = GetCurrentUserId();
            return _context.UserStationPermissions
                .Where(p => p.UserId == userId)
                .Select(p => p.StationId)
                .ToList();
        }

        public bool HasStationPermission(int stationId)
        {
            if (IsAdmin())
            {
                return true;
            }

            var availableStationIds = GetAvailableStationIds();
            return availableStationIds.Contains(stationId);
        }

        public string HashPassword(ApplicationUser user, string password)
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            return hasher.HashPassword(user, password);
        }

        public bool VerifyPassword(ApplicationUser user, string password, string hashedPassword)
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            var result = hasher.VerifyHashedPassword(user, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ResourceNotFoundException("使用者", userId);
            }
            return user;
        }

        public async Task<ApplicationUser> CreateUserAsync(string email, string username, string name, string password, string role = "User")
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                Name = name,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"建立使用者失敗: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = role });
            }

            // Add user to role
            await _userManager.AddToRoleAsync(user, role);

            return user;
        }

        public async Task AddUserToStationAsync(string userId, int stationId)
        {
            var existingPermission = await _context.UserStationPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.StationId == stationId);

            if (existingPermission == null)
            {
                _context.UserStationPermissions.Add(new UserStationPermission
                {
                    UserId = userId,
                    StationId = stationId
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromStationAsync(string userId, int stationId)
        {
            var permission = await _context.UserStationPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.StationId == stationId);

            if (permission != null)
            {
                _context.UserStationPermissions.Remove(permission);
                await _context.SaveChangesAsync();
            }
        }
    }
}