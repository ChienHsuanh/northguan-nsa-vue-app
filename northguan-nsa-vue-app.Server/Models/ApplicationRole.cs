using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace northguan_nsa_vue_app.Server.Models
{
    /// <summary>
    /// 應用程式角色
    /// </summary>
    public class ApplicationRole : IdentityRole<string>
    {
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string? Description { get; set; }
    }
}