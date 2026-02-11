using System.ComponentModel.DataAnnotations;
using northguan_nsa_vue_app.Server.Resources;

namespace northguan_nsa_vue_app.Server.DTOs
{
    public class CreateAccountRequest
    {
        [Required(ErrorMessage = ValidationMessages.Specific.NameRequired)]
        [StringLength(100, ErrorMessage = ValidationMessages.StringLengthMax)]
        public required string Name { get; set; }

        [StringLength(50, ErrorMessage = ValidationMessages.StringLengthMax)]
        public string? Username { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = ValidationMessages.Specific.PasswordLength)]
        public string? Password { get; set; }

        public string? Role { get; set; }

        [Phone(ErrorMessage = ValidationMessages.Phone)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "員工編號為必填欄位")]
        [StringLength(20, ErrorMessage = ValidationMessages.StringLengthMax)]
        public required string EmployeeId { get; set; }

        [Url(ErrorMessage = ValidationMessages.Url)]
        public string? AvatarUrl { get; set; }

        public bool IsReadOnly { get; set; }
        public int[]? StationIds { get; set; }
    }

    public class UpdateAccountRequest
    {
        [StringLength(100, ErrorMessage = ValidationMessages.StringLengthMax)]
        public string? Name { get; set; }

        [StringLength(50, ErrorMessage = ValidationMessages.StringLengthMax)]
        public string? Username { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = ValidationMessages.Specific.PasswordLength)]
        public string? Password { get; set; }

        public string? Role { get; set; }

        [Phone(ErrorMessage = ValidationMessages.Phone)]
        public string? Phone { get; set; }

        [StringLength(20, ErrorMessage = ValidationMessages.StringLengthMax)]
        public string? EmployeeId { get; set; }

        [Url(ErrorMessage = ValidationMessages.Url)]
        public string? AvatarUrl { get; set; }

        public bool? IsReadOnly { get; set; }
        public List<int>? StationIds { get; set;}
    }
}