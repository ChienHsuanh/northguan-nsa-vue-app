using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 排程任務服務接口
    /// </summary>
    public interface IScheduledTaskService
    {
        /// <summary>
        /// 檢查設備在線狀態
        /// </summary>
        Task<DeviceOnlineCheckResponse> CheckDevicesOnlineAsync(bool forceExecution = false);

        /// <summary>
        /// 同步人流設備數據
        /// </summary>
        Task<CrowdDataSyncResponse> SyncCrowdDeviceDataAsync(bool forceExecution = false);

        /// <summary>
        /// 同步停車記錄
        /// </summary>
        Task<ParkingDataSyncResponse> SyncParkingRecordAsync(bool forceExecution = false);

        /// <summary>
        /// 同步交通設備數據
        /// </summary>
        Task<TrafficDataSyncResponse> SyncTrafficDeviceDataAsync(bool forceExecution = false);

        /// <summary>
        /// 同步交通 CVP 數據
        /// </summary>
        Task SyncTrafficCvpDataAsync();

        /// <summary>
        /// 同步 E2 CVP 數據
        /// </summary>
        Task SyncE2CvpDataAsync();

        /// <summary>
        /// 同步 G2 CVP 數據
        /// </summary>
        Task SyncG2CvpDataAsync();

        /// <summary>
        /// 同步零接觸訪客數據
        /// </summary>
        Task SyncZeroTouchVisitorAsync(bool forceExecution = false);

        /// <summary>
        /// 備份警告日誌
        /// </summary>
        Task<LogBackupResponse> BackupWarningLogAsync(bool forceExecution = false);

        /// <summary>
        /// 檢查審計日誌
        /// </summary>
        Task<AuditLogCleanupResponse> CheckAuditLogAsync(bool forceExecution = false);

        /// <summary>
        /// 備份資料庫
        /// </summary>
        Task<DatabaseBackupResponse> BackupDatabaseAsync(bool forceExecution = false);

        /// <summary>
        /// 執行指定的排程任務
        /// </summary>
        Task<object> ExecuteScheduledTaskAsync(string taskName, bool forceExecution = false);

        /// <summary>
        /// 獲取所有可用的排程任務列表
        /// </summary>
        List<string> GetAvailableScheduledTasks();
    }
}