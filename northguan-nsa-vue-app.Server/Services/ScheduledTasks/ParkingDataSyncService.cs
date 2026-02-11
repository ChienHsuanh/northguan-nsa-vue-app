using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services.ExternalApi;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 停車數據同步服務
    /// </summary>
    public class ParkingDataSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly ParkingDataApiService _parkingApi;
        private readonly TransportationUploadService _transportationUpload;
        private readonly ICacheService _cache;
        private readonly ILogger<ParkingDataSyncService> _logger;

        // 數據同步間隔 (5分鐘)
        private static readonly TimeSpan SYNC_INTERVAL = TimeSpan.FromMinutes(5);

        public ParkingDataSyncService(
            ApplicationDbContext context,
            ParkingDataApiService parkingApi,
            TransportationUploadService transportationUpload,
            ICacheService cache,
            ILogger<ParkingDataSyncService> logger)
        {
            _context = context;
            _parkingApi = parkingApi;
            _transportationUpload = transportationUpload;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 同步停車記錄
        /// </summary>
        public async Task<ParkingDataSyncResponse> SyncParkingRecordAsync(bool forceExecution = false)
        {
            var response = new ParkingDataSyncResponse
            {
                Success = false,
                Message = "停車記錄同步失敗"
            };

            try
            {
                _logger.LogInformation("開始同步停車記錄");

                var devices = await _context.ParkingDevices.ToListAsync();
                var newRecords = new List<ParkingRecord>();
                var errors = new List<string>();
                var currentTime = DateTime.Now;

                foreach (var device in devices)
                {
                    if (!forceExecution && await ShouldSkipSync($"sync-parking-{device.Serial}", currentTime))
                        continue;

                    try
                    {
                        var systemType = ParkingSystemDetector.DetectSystemType(device.ApiUrl ?? "");
                        var data = await _parkingApi.FetchParkingDataAsync(device.ApiUrl ?? "", device.Serial ?? "", device.NumberOfParking, systemType);
                        if (data == null) continue;

                        var newRecord = CreateParkingRecord(device, data, currentTime);
                        newRecords.Add(newRecord);

                        await UpdateDeviceOnlineStatus(device, currentTime);
                        await _cache.SetAsync($"sync-parking-{device.Serial}", currentTime, SYNC_INTERVAL);

                        // 上傳到交通部 API
                        await _transportationUpload.UploadParkingDataAsync(
                            newRecord.DeviceSerial,
                            newRecord.AvailableSpaces,
                            newRecord.TotalSpaces);
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"同步停車設備數據失敗: {device.Serial} - {ex.Message}";
                        _logger.LogError(ex, errorMsg);
                        errors.Add(errorMsg);
                    }
                }

                if (newRecords.Any())
                {
                    _context.ParkingRecords.AddRange(newRecords);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("成功同步 {Count} 筆停車記錄", newRecords.Count);
                }

                response.Success = true;
                response.Message = "停車記錄同步已完成";
                response.SyncedRecords = newRecords.Count;
                response.FailedRecords = errors.Count;
                response.Errors = errors;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "同步停車記錄時發生錯誤");
                response.Message = $"同步停車記錄時發生錯誤: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        #region Private Methods

        private async Task<bool> ShouldSkipSync(string cacheKey, DateTime currentTime)
        {
            if (!await _cache.ExistsAsync(cacheKey))
                return false;

            var lastExec = await _cache.GetAsync<DateTime?>(cacheKey);
            return lastExec.HasValue && currentTime - lastExec.Value < SYNC_INTERVAL;
        }

        private static ParkingRecord CreateParkingRecord(ParkingDevice device, ParkingApiResponse data, DateTime currentTime)
        {
            return new ParkingRecord
            {
                DeviceSerial = device.Serial ?? "",
                Time = currentTime,
                TotalSpaces = device.NumberOfParking,  // Fix: Use device's total parking spaces
                OccupiedSpaces = data.ParkedNum,
                AvailableSpaces = data.RemainingNum,
                OccupancyRate = device.NumberOfParking > 0 ? (decimal)data.ParkedNum / device.NumberOfParking * 100 : 0,  // Fix: Use correct calculation
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };
        }

        private async Task UpdateDeviceOnlineStatus(ParkingDevice device, DateTime currentTime)
        {
            device.Status = "online";
            device.LatestOnlineTime = currentTime;

            await UpdateDeviceStatusLog("parking", device.Serial ?? "");
        }

        private async Task UpdateDeviceStatusLog(string deviceType, string serial)
        {
            var statusLog = new DeviceStatusLog
            {
                DeviceType = deviceType,
                DeviceSerial = serial,
                Status = "online",
                Timestamp = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.DeviceStatusLogs.Add(statusLog);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}