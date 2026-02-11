using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 主要排程任務服務 - 協調各個子服務
    /// </summary>
    public class ScheduledTaskService : IScheduledTaskService
    {
        private readonly DeviceOnlineCheckService _deviceOnlineCheck;
        private readonly DataSyncService _dataSync;
        private readonly BackupService _backup;
        private readonly CvpDataSyncService _cvpDataSync;
        private readonly ILogger<ScheduledTaskService> _logger;

        public ScheduledTaskService(
            DeviceOnlineCheckService deviceOnlineCheck,
            DataSyncService dataSync,
            BackupService backup,
            CvpDataSyncService cvpDataSync,
            ILogger<ScheduledTaskService> logger)
        {
            _deviceOnlineCheck = deviceOnlineCheck;
            _dataSync = dataSync;
            _backup = backup;
            _cvpDataSync = cvpDataSync;
            _logger = logger;
        }

        public async Task<DeviceOnlineCheckResponse> CheckDevicesOnlineAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行設備在線狀態檢查 (強制執行: {ForceExecution})", forceExecution);
            return await _deviceOnlineCheck.CheckDevicesOnlineAsync(forceExecution);
        }

        public async Task<CrowdDataSyncResponse> SyncCrowdDeviceDataAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行人流設備數據同步 (強制執行: {ForceExecution})", forceExecution);
            return await _dataSync.SyncCrowdDeviceDataAsync(forceExecution);
        }

        public async Task<ParkingDataSyncResponse> SyncParkingRecordAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行停車記錄同步 (強制執行: {ForceExecution})", forceExecution);
            return await _dataSync.SyncParkingRecordAsync(forceExecution);
        }

        public async Task<TrafficDataSyncResponse> SyncTrafficDeviceDataAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行交通設備數據同步 (強制執行: {ForceExecution})", forceExecution);
            return await _dataSync.SyncTrafficDeviceDataAsync(forceExecution);
        }

        public async Task SyncTrafficCvpDataAsync()
        {
            _logger.LogDebug("執行交通 CVP 數據同步");
            await _cvpDataSync.SyncTrafficCvpDataAsync();
        }

        public async Task SyncE2CvpDataAsync()
        {
            _logger.LogDebug("執行 E2 CVP 數據同步");
            await _cvpDataSync.SyncE2CvpDataAsync();
        }

        public async Task SyncG2CvpDataAsync()
        {
            _logger.LogDebug("執行 G2 CVP 數據同步");
            await _cvpDataSync.SyncG2CvpDataAsync();
        }

        public async Task SyncZeroTouchVisitorAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行零接觸訪客數據同步");
            await _backup.SyncZeroTouchVisitorAsync(forceExecution);
        }

        public async Task<LogBackupResponse> BackupWarningLogAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行警告日誌備份 (強制執行: {ForceExecution})", forceExecution);
            return await _backup.BackupWarningLogAsync(forceExecution);
        }

        public async Task<AuditLogCleanupResponse> CheckAuditLogAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行審計日誌檢查 (強制執行: {ForceExecution})", forceExecution);
            return await _backup.CheckAuditLogAsync(forceExecution);
        }

        public async Task<DatabaseBackupResponse> BackupDatabaseAsync(bool forceExecution = false)
        {
            _logger.LogDebug("執行資料庫備份 (強制執行: {ForceExecution})", forceExecution);
            return await _backup.BackupDatabaseAsync(forceExecution);
        }

        public async Task<object> ExecuteScheduledTaskAsync(string taskName, bool forceExecution = false)
        {
            _logger.LogInformation("執行排程任務: {TaskName} (強制執行: {ForceExecution})", taskName, forceExecution);

            return taskName.ToLowerInvariant() switch
            {
                "check-devices-online" => await CheckDevicesOnlineAsync(forceExecution),
                "sync-crowd-data" => await SyncCrowdDeviceDataAsync(forceExecution),
                "sync-parking-data" => await SyncParkingRecordAsync(forceExecution),
                "sync-traffic-data" => await SyncTrafficDeviceDataAsync(forceExecution),
                "sync-traffic-cvp-data" => await ExecuteVoidTaskAsync(() => SyncTrafficCvpDataAsync()),
                "sync-e2-cvp-data" => await ExecuteVoidTaskAsync(() => SyncE2CvpDataAsync()),
                "sync-g2-cvp-data" => await ExecuteVoidTaskAsync(() => SyncG2CvpDataAsync()),
                "sync-zero-touch-visitor" => await ExecuteVoidTaskAsync(() => SyncZeroTouchVisitorAsync()),
                "backup-database" => await BackupDatabaseAsync(forceExecution),
                "backup-logs" => await BackupWarningLogAsync(forceExecution),
                "cleanup-audit-logs" => await CheckAuditLogAsync(),
                _ => throw new ArgumentException($"未知的排程任務: {taskName}", nameof(taskName))
            };
        }

        public List<string> GetAvailableScheduledTasks()
        {
            return new List<string>
            {
                "check-devices-online",
                "sync-crowd-data",
                "sync-parking-data",
                "sync-traffic-data",
                "sync-traffic-cvp-data",
                "sync-e2-cvp-data",
                "sync-g2-cvp-data",
                "sync-zero-touch-visitor",
                "backup-database",
                "backup-logs",
                "cleanup-audit-logs"
            };
        }

        private async Task<object> ExecuteVoidTaskAsync(Func<Task> taskFunc)
        {
            await taskFunc();
            return new { Success = true, Message = "任務執行完成", ExecutedAt = DateTime.Now };
        }
    }
}