using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Controllers;
using northguan_nsa_vue_app.Server.DTOs;
using Microsoft.AspNetCore.Identity;
using northguan_nsa_vue_app.Server.Exceptions;

namespace northguan_nsa_vue_app.Server.Services
{
  public class AccountService : IAccountService
  {
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountService(ApplicationDbContext context, IAuthService authService, UserManager<ApplicationUser> userManager)
    {
      _context = context;
      _authService = authService;
      _userManager = userManager;
    }


    public async Task<List<UserProfileResponse>> GetAccountListAsync(int page, int size, string keyword)
    {
      var users = _userManager.Users;

      if (!string.IsNullOrEmpty(keyword))
      {
        users = users.Where(u =>
          u.Name.Contains(keyword) ||
          (u.UserName != null && u.UserName.Contains(keyword)) ||
          (u.Email != null && u.Email.Contains(keyword))
        );
      }

      var totalCount = await users.CountAsync();
      var skip = (page - 1) * size;
      var userList = await users.OrderBy(u => u.Name)
          .Skip(skip)
          .Take(size)
          .ToListAsync();

      var userProfiles = new List<UserProfileResponse>();
      foreach (var user in userList)
      {
        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "User";

        userProfiles.Add(new UserProfileResponse
        {
          Id = user.Id,
          Name = user.Name,
          Username = user.UserName,
          Email = user.Email,
          Role = userRole,
          Phone = user.PhoneNumber,
          EmployeeId = user.EmployeeId,
          AvatarFilename = user.AvatarFilename,
          IsReadOnly = user.ReadOnly,
          StationIds = _context.UserStationPermissions
                .Where(p => p.UserId == user.Id)
                .Select(p => p.StationId)
                .ToList()
        });
      }

      return userProfiles;
    }

    public async Task<int> GetAccountCountAsync(string keyword)
    {
      var users = _userManager.Users;
      if (!string.IsNullOrEmpty(keyword))
      {
        users = users.Where(u => u.Name.Contains(keyword) || (u.UserName != null && u.UserName.Contains(keyword)) || (u.Email != null && u.Email.Contains(keyword)));
      }

      var accountCount = await _userManager.Users.CountAsync();
      return accountCount;
    }

    public async Task CreateAccountAsync(CreateAccountRequest request)
    {
      var strategy = _context.Database.CreateExecutionStrategy();
      await strategy.ExecuteAsync(async () =>
      {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
          var user = new ApplicationUser
          {
            Name = request.Name,
            UserName = request.Username,
            Email = request.Username,
            PhoneNumber = request.Phone,
            EmployeeId = request.EmployeeId,
            ReadOnly = request.IsReadOnly
          };

          if (request.AvatarUrl != null)
          {
            // TODO: save avatar to file system
            // user.AvatarFilename = await _authService.SaveAvatarAsync(request.AvatarUrl, user.Id);
          }

          IdentityResult result;
          if (string.IsNullOrEmpty(request.Password))
          {
            result = await _userManager.CreateAsync(user);
          }
          else
          {
            result = await _userManager.CreateAsync(user, request.Password);
          }

          if (!result.Succeeded)
          {
              await transaction.RollbackAsync();
              throw new InvalidOperationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
          }

          // Ensure the user is persisted before adding roles
          await _context.SaveChangesAsync(); // Explicitly save changes

          // 設定角色
          if (!string.IsNullOrEmpty(request.Role))
          {
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to add user to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
          }
          else
          {
            // 預設為 User 角色
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to add user to default role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
          }

          if (request.StationIds != null)
          {
            foreach (var stationId in request.StationIds)
            {
              await _authService.AddUserToStationAsync(user.Id, stationId);
            }
          }
          await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
          await transaction.RollbackAsync();
          throw;
        }
      });
    }

    public async Task UpdateAccountAsync(string id, UpdateAccountRequest request)
    {
      var user = await _userManager.FindByIdAsync(id)
        ?? throw new ResourceNotFoundException("使用者", id);

      // 更新使用者資訊
      user.Name = request.Name ?? user.Name;
      user.UserName = request.Username ?? user.UserName;
      user.Email = request.Username ?? user.Email;
      user.PhoneNumber = request.Phone ?? user.PhoneNumber;
      user.EmployeeId = request.EmployeeId ?? user.EmployeeId;
      user.ReadOnly = request.IsReadOnly ?? user.ReadOnly;

      if (!string.IsNullOrEmpty(request.Password))
      {
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
          var removePasswordResult = await _userManager.RemovePasswordAsync(user);
          if (!removePasswordResult.Succeeded)
          {
              throw new InvalidOperationException($"Failed to remove old password: {string.Join(", ", removePasswordResult.Errors.Select(e => e.Description))}");
          }
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password);
        if (!addPasswordResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add new password: {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}");
        }
      }

      // 更新身分組
      if (request.Role != null)
      {
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count() > 0)
        {
          var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, roles);
          if (!removeRoleResult.Succeeded)
          {
              throw new InvalidOperationException($"Failed to remove user from existing roles: {string.Join(", ", removeRoleResult.Errors.Select(e => e.Description))}");
          }
        }
        var addRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add user to new role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
        }
      }

      bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

      // 更新站點權限
      var currentStationIds = _context.UserStationPermissions
          .Where(p => p.UserId == user.Id)
          .Select(p => p.StationId)
          .ToList();

      var newStationIds = request.StationIds;
      if (!isAdmin)
      {
        var toAdd = newStationIds?.Except(currentStationIds) ?? new List<int>();
        foreach (var stationId in toAdd)
        {
          await _authService.AddUserToStationAsync(user.Id, stationId);
        }
      }
      var toRemove = currentStationIds.Except(newStationIds ?? new List<int>());
      foreach (var stationId in toRemove)
      {
          await _authService.RemoveUserFromStationAsync(user.Id, stationId);
      }

      var updateResult = await _userManager.UpdateAsync(user);
      if (!updateResult.Succeeded)
      {
          throw new InvalidOperationException($"Failed to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
      }
    }

    public async Task DeleteAccountAsync(string id)
    {
      var user = await _userManager.FindByIdAsync(id)
        ?? throw new ResourceNotFoundException($"使用者 {id} 不存在");

      var result = await _userManager.DeleteAsync(user);
      if (!result.Succeeded)
      {
        throw new InvalidOperationException($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
      }
    }
  }
}