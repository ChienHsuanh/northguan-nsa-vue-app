using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace northguan_nsa_vue_app.Server.Models
{
    /// <summary>
    /// 圍籬事件類型
    /// </summary>
    public enum FenceEventType
    {
        /// <summary>
        /// 闖入事件
        /// </summary>
        Enter = 1,

        /// <summary>
        /// 離開事件
        /// </summary>
        Exit = 2
    }

    /// <summary>
    /// 停車記錄
    /// </summary>
    [Table("ParkingRecords")]
    public class ParkingRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "int")]
        public int TotalSpaces { get; set; }

        [Column(TypeName = "int")]
        public int OccupiedSpaces { get; set; }

        [Column(TypeName = "int")]
        public int AvailableSpaces { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal OccupancyRate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ParkingDevice? ParkingDevice { get; set; }
    }

    /// <summary>
    /// 人流記錄
    /// </summary>
    [Table("CrowdRecords")]
    public class CrowdRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "int")]
        public int TotalIn { get; set; }

        [Column(TypeName = "int")]
        public int TotalOut { get; set; }

        [Column(TypeName = "int")]
        public int In { get; set; }

        [Column(TypeName = "int")]
        public int Out { get; set; }

        [Column(TypeName = "int")]
        public int Count { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual CrowdDevice? CrowdDevice { get; set; }
    }

    /// <summary>
    /// 人流最新記錄
    /// </summary>
    [Table("CrowdRecordLatests")]
    public class CrowdRecordLatest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "int")]
        public int TotalIn { get; set; }

        [Column(TypeName = "int")]
        public int TotalOut { get; set; }

        [Column(TypeName = "int")]
        public int Count { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 交通記錄
    /// </summary>
    [Table("TrafficRecords")]
    public class TrafficRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "int")]
        public int VehicleCount { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal AverageSpeed { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal TravelTime { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string TrafficCondition { get; set; } = "normal";

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual TrafficDevice? TrafficDevice { get; set; }
    }

    /// <summary>
    /// 圍籬記錄
    /// </summary>
    [Table("FenceRecords")]
    public class FenceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "int")]
        public FenceEventType EventType { get; set; }

        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? Photo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual FenceDevice? FenceDevice { get; set; }
    }

    /// <summary>
    /// 設備狀態日誌
    /// </summary>
    [Table("DeviceStatusLogs")]
    public class DeviceStatusLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string DeviceType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Timestamp { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 零接觸訪客記錄
    /// </summary>
    [Table("ZeroTouchRecords")]
    public class ZeroTouchRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string VisitorId { get; set; } = string.Empty;

        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string? VisitorName { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime VisitTime { get; set; }

        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string? Location { get; set; }

        [StringLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string? Purpose { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 高解析度設備記錄
    /// </summary>
    [Table("HighResolutionRecords")]
    public class HighResolutionRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? ImagePath { get; set; }

        [StringLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        public string? Description { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// E CVP 遊客記錄
    /// </summary>
    [Table("ECvpTouristRecords")]
    public class ECvpTouristRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "int")]
        public int TouristCount { get; set; }

        [StringLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        public string? LocationData { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 入場記錄
    /// </summary>
    [Table("AdmissionRecords")]
    public class AdmissionRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string DeviceSerial { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime Time { get; set; }

        [Column(TypeName = "int")]
        public int AdmissionCount { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string AdmissionType { get; set; } = "entry";

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}