using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 數據同步服務協調器
    /// </summary>
    public class DataSyncService
    {
        private readonly CrowdDataSyncService _crowdDataSync;
        private readonly ParkingDataSyncService _parkingDataSync;
        private readonly TrafficDataSyncService _trafficDataSync;
        private readonly ILogger<DataSyncService> _logger;

        public DataSyncService(
            CrowdDataSyncService crowdDataSync,
            ParkingDataSyncService parkingDataSync,
            TrafficDataSyncService trafficDataSync,
            ILogger<DataSyncService> logger)
        {
            _crowdDataSync = crowdDataSync;
            _parkingDataSync = parkingDataSync;
            _trafficDataSync = trafficDataSync;
            _logger = logger;
        }

        public async Task<CrowdDataSyncResponse> SyncCrowdDeviceDataAsync(bool forceExecution = false)
        {
            _logger.LogDebug("委派人流設備數據同步 (強制執行: {ForceExecution})", forceExecution);
            return await _crowdDataSync.SyncCrowdDeviceDataAsync(forceExecution);
        }

        public async Task<ParkingDataSyncResponse> SyncParkingRecordAsync(bool forceExecution = false)
        {
            _logger.LogDebug("委派停車記錄同步 (強制執行: {ForceExecution})", forceExecution);
            return await _parkingDataSync.SyncParkingRecordAsync(forceExecution);
        }

        public async Task<TrafficDataSyncResponse> SyncTrafficDeviceDataAsync(bool forceExecution = false)
        {
            _logger.LogDebug("委派交通設備數據同步 (強制執行: {ForceExecution})", forceExecution);
            return await _trafficDataSync.SyncTrafficDeviceDataAsync(forceExecution);
        }
    }
}