using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace northguan_nsa_vue_app.Server.Models
{
    /// <summary>
    /// 站點
    /// </summary>
    [Table("Stations")]
    public class Station
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,8)")]
        public decimal Lat { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal Lng { get; set; }

        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? LineToken { get; set; }

        [Column(TypeName = "bit")]
        public bool EnableNotify { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? CvpLocations { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<CrowdDevice> CrowdDevices { get; set; } = new List<CrowdDevice>();
        public virtual ICollection<ParkingDevice> ParkingDevices { get; set; } = new List<ParkingDevice>();
        public virtual ICollection<TrafficDevice> TrafficDevices { get; set; } = new List<TrafficDevice>();
        public virtual ICollection<FenceDevice> FenceDevices { get; set; } = new List<FenceDevice>();
        public virtual ICollection<HighResolutionDevice> HighResolutionDevices { get; set; } = new List<HighResolutionDevice>();
    }
}