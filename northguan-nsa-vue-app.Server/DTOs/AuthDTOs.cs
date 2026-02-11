using System.ComponentModel.DataAnnotations;
using northguan_nsa_vue_app.Server.Resources;

namespace northguan_nsa_vue_app.Server.DTOs
{
    public class PasswordLoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public string? Role { get; set; } = "User";
        public string? Phone { get; set; }
        public string? EmployeeId { get; set; }
        public bool ReadOnly { get; set; } = false;
        public List<int>? StationIds { get; set; }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = ValidationMessages.Specific.UsernameRequired)]
        [StringLength(50, ErrorMessage = ValidationMessages.StringLengthMax)]
        public required string Username { get; set; }
        
        [Required(ErrorMessage = ValidationMessages.Specific.PasswordRequired)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = ValidationMessages.Specific.PasswordLength)]
        public required string Password { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
        public required UserResponse User { get; set; }
    }

    public class UserResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public string? Phone { get; set; }
        public string? EmployeeId { get; set; }
        public bool IsReadOnly { get; set; }
        public List<int>? StationIds { get; set; }
    }

    public class ProfileResponse
    {
        public required string Name { get; set; }
        public string? Username { get; set; }
        public required string Role { get; set; }
        public string? Phone { get; set; }
        public string? EmployeeID { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsReadOnly { get; set; }
        public List<int>? StationIds { get; set; }
    }

    public class UserProfileResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public required string Role { get; set; }
        public string? Phone { get; set; }
        public string? EmployeeId { get; set; }
        public string? AvatarFilename { get; set; }
        public bool IsReadOnly { get; set; }
        public List<int>? StationIds { get; set; }
    }

    public class UpdateUserProfileRequest
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? EmployeeId { get; set; }
        public string? Password { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }

    public class ChangePasswordRequest
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class UserStationPermissionRequest
    {
        public required string UserId { get; set; }
        public required List<int> StationIds { get; set; }
    }
}