using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace northguan_nsa_vue_app.Server.Models
{
    /// <summary>
    /// 應用程式使用者
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // 讓密碼可為空
        public override string? PasswordHash { get; set; }

        [Required]
        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? Phone { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? EmployeeId { get; set; }

        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? AvatarFilename { get; set; }

        [Column(TypeName = "bit")]
        public bool ReadOnly { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties for station permissions
        public virtual ICollection<UserStationPermission> StationPermissions { get; set; } = new List<UserStationPermission>();
    }

    /// <summary>
    /// 使用者站點權限
    /// </summary>
    [Table("UserStationPermissions")]
    public class UserStationPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        public string UserId { get; set; } = string.Empty;

        [Column(TypeName = "int")]
        public int StationId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("StationId")]
        public virtual Station Station { get; set; } = null!;
    }
}