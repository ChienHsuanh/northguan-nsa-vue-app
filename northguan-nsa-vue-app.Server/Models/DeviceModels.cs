using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace northguan_nsa_vue_app.Server.Models
{
    /// <summary>
    /// 設備基礎類別
    /// </summary>
    public abstract class BaseDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "int")]
        public int StationId { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public required string Serial { get; set; }

        [Column(TypeName = "decimal(10,8)")]
        public decimal Lat { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal Lng { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? Status { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LatestOnlineTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 攝影機設定 JSON，含 RtspUrl、類型、PTZ 參數等
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? CameraConfig { get; set; }

        // Navigation property
        [ForeignKey("StationId")]
        public virtual Station Station { get; set; } = null!;
    }

    /// <summary>
    /// 人流設備
    /// </summary>
    [Table("CrowdDevices")]
    public class CrowdDevice : BaseDevice
    {
        [Column(TypeName = "int")]
        [Comment("面積(平方公尺)")]
        public int Area { get; set; }

        [StringLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string? VideoUrl { get; set; }

        [StringLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string? ApiUrl { get; set; }
    }

    /// <summary>
    /// 停車設備
    /// </summary>
    [Table("ParkingDevices")]
    public class ParkingDevice : BaseDevice
    {
        [StringLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string? ApiUrl { get; set; }

        [Column(TypeName = "int")]
        [Comment("停車位數")]
        public int NumberOfParking { get; set; }
    }

    /// <summary>
    /// 交通設備
    /// </summary>
    [Table("TrafficDevices")]
    public class TrafficDevice : BaseDevice
    {
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? City { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? ETagNumber { get; set; }

        [Column(TypeName = "int")]
        public int SpeedLimit { get; set; }
    }

    /// <summary>
    /// 圍籬設備
    /// </summary>
    [Table("FenceDevices")]
    public class FenceDevice : BaseDevice
    {
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? ObservingTimeStart { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? ObservingTimeEnd { get; set; }

        [StringLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string? VideoUrl { get; set; }

        /// <summary>
        /// 圍籬多邊形區域 JSON，結構: [{"Name":"禁區1","Points":[{"X":0.1,"Y":0.2}...],"Cooldown":30,"Enabled":true}]
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? Zones { get; set; }
    }

    /// <summary>
    /// 高解析度設備
    /// </summary>
    [Table("HighResolutionDevices")]
    public class HighResolutionDevice : BaseDevice
    {
        [StringLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string? VideoUrl { get; set; }
    }

    /// <summary>
    /// 水域監控設備
    /// </summary>
    [Table("WaterDevices")]
    public class WaterDevice : BaseDevice
    {
        [StringLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string? VideoUrl { get; set; }
    }

}