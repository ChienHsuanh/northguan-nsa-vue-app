using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAccountService _accountService;
        private readonly IFileService _fileService;

        public AuthController(IAuthService authService, IAccountService accountService, IFileService fileService)
        {
            _authService = authService;
            _accountService = accountService;
            _fileService = fileService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request.Username, request.Password);
            return Ok(response);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ProfileResponse>> GetProfile()
        {
            var user = await _authService.GetCurrentUserAsync();

            var avatarUrl = "";
            if (!string.IsNullOrEmpty(user.AvatarFilename))
            {
                avatarUrl = _fileService.GetUploadFileUrl("avatar", user.AvatarFilename);
            }

            return Ok(new ProfileResponse
            {
                Name = user.Name,
                Username = user.UserName,
                Role = _authService.GetCurrentUserRole(),
                Phone = user.Phone,
                EmployeeID = user.EmployeeId,
                AvatarUrl = avatarUrl,
                IsReadOnly = user.ReadOnly
            });
        }

        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request)
        {
            var user = await _authService.GetCurrentUserAsync();

            var avatarUrl = "";
            if (!string.IsNullOrEmpty(user.AvatarFilename))
            {
                avatarUrl = _fileService.GetUploadFileUrl("avatar", user.AvatarFilename);
            }

            var updateAccountRequest = new UpdateAccountRequest
            {
                Name = request.Name,
                Phone = request.Phone,
                EmployeeId = request.EmployeeId,
                Password = request.Password,
                AvatarUrl = avatarUrl,
                IsReadOnly = user.ReadOnly
            };

            await _accountService.UpdateAccountAsync(user.Id, updateAccountRequest);
            return NoContent();
        }
    }
}