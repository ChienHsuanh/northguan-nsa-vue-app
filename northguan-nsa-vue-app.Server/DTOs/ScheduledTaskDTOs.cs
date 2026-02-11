using System.ComponentModel.DataAnnotations;

namespace northguan_nsa_vue_app.Server.DTOs
{
    /// <summary>
    /// 人流設備數據響應
    /// </summary>
    public class CrowdDeviceDataResponse
    {
        public string? Timestamp { get; set; }
        public int TotalIn { get; set; }
        public int TotalOut { get; set; }
        public int Occupancy { get; set; }
    }

    /// <summary>
    /// 停車設備數據響應
    /// </summary>
    public class ParkingDeviceDataResponse
    {
        public string? Timestamp { get; set; }
        public int TotalSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public double OccupancyRate { get; set; }
    }

    /// <summary>
    /// 交通設備數據響應
    /// </summary>
    public class TrafficDeviceDataResponse
    {
        public string? Timestamp { get; set; }
        public int VehicleCount { get; set; }
        public double AverageSpeed { get; set; }
        public string? TrafficCondition { get; set; }
    }

    /// <summary>
    /// CVP 設備數據響應
    /// </summary>
    public class CvpDeviceDataResponse
    {
        public string? Timestamp { get; set; }
        public int TotalCount { get; set; }
        public List<CvpLocationData>? LocationData { get; set; }
    }

    /// <summary>
    /// CVP 位置數據
    /// </summary>
    public class CvpLocationData
    {
        public string? LocationId { get; set; }
        public int Count { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>
    /// 設備離線資訊
    /// </summary>
    public class DeviceOfflineInfo
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceSerial { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public DateTime LastOnlineTime { get; set; }
    }

    /// <summary>
    /// 同步任務結果
    /// </summary>
    public class SyncTaskResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int ProcessedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// 設備狀態更新請求
    /// </summary>
    public class DeviceStatusUpdateRequest
    {
        [Required]
        public string DeviceSerial { get; set; } = string.Empty;
        
        [Required]
        public string DeviceType { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public DateTime? LastOnlineTime { get; set; }
    }

    /// <summary>
    /// 備份任務配置
    /// </summary>
    public class BackupTaskConfig
    {
        public string BackupPath { get; set; } = string.Empty;
        public string DatabaseConnectionString { get; set; } = string.Empty;
        public int RetentionDays { get; set; } = 30;
        public bool CompressBackup { get; set; } = true;
    }

    /// <summary>
    /// 日誌清理配置
    /// </summary>
    public class LogCleanupConfig
    {
        public string LogDirectory { get; set; } = string.Empty;
        public int RetentionDays { get; set; } = 30;
        public List<string> FilePatterns { get; set; } = new();
    }

    /// <summary>
    /// 零接觸訪客數據
    /// </summary>
    public class ZeroTouchVisitorData
    {
        public string VisitorId { get; set; } = string.Empty;
        public string VisitorName { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }

    /// <summary>
    /// 排程任務狀態
    /// </summary>
    public class ScheduledTaskStatus
    {
        public string TaskName { get; set; } = string.Empty;
        public DateTime LastRunTime { get; set; }
        public DateTime? NextRunTime { get; set; }
        public bool IsRunning { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? LastError { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    /// <summary>
    /// 系統健康檢查響應
    /// </summary>
    public class SystemHealthResponse
    {
        public bool IsHealthy { get; set; }
        public List<ScheduledTaskStatus> TaskStatuses { get; set; } = new();
        public Dictionary<string, object> SystemInfo { get; set; } = new();
        public DateTime CheckTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 設備在線檢查響應
    /// </summary>
    public class DeviceOnlineCheckResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalDevicesChecked { get; set; }
        public int OnlineDevices { get; set; }
        public int OfflineDevices { get; set; }
        public List<DeviceOfflineInfo> OfflineDeviceList { get; set; } = new();
        public DateTime CheckTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 人流數據同步響應
    /// </summary>
    public class CrowdDataSyncResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SyncedRecords { get; set; }
        public int FailedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SyncTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 停車數據同步響應
    /// </summary>
    public class ParkingDataSyncResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SyncedRecords { get; set; }
        public int FailedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SyncTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 交通數據同步響應
    /// </summary>
    public class TrafficDataSyncResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SyncedRecords { get; set; }
        public int FailedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime SyncTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 資料庫備份響應
    /// </summary>
    public class DatabaseBackupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string BackupFilePath { get; set; } = string.Empty;
        public long BackupFileSize { get; set; }
        public TimeSpan BackupDuration { get; set; }
        public DateTime BackupTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 日誌備份響應
    /// </summary>
    public class LogBackupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int BackedUpFiles { get; set; }
        public long TotalBackupSize { get; set; }
        public string BackupLocation { get; set; } = string.Empty;
        public DateTime BackupTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 審計日誌清理響應
    /// </summary>
    public class AuditLogCleanupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CleanedFiles { get; set; }
        public int FreedSpace { get; set; }
        public int RetentionDays { get; set; }
        public DateTime CleanupTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// TDX 車流設備狀態檢查響應
    /// </summary>
    public class TdxDeviceStatusCheckResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalTdxDevices { get; set; }
        public int OnlineTdxDevices { get; set; }
        public int OfflineTdxDevices { get; set; }
        public List<DeviceOfflineInfo> OfflineDeviceList { get; set; } = new();
        public DateTime CheckTime { get; set; } = DateTime.Now;
    }
}