using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services.ExternalApi;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 人流數據同步服務
    /// </summary>
    public class CrowdDataSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly CrowdDataApiService _crowdApi;
        private readonly TransportationUploadService _transportationUpload;
        private readonly ICacheService _cache;
        private readonly ILogger<CrowdDataSyncService> _logger;

        // 數據同步間隔 (5分鐘)
        private static readonly TimeSpan SYNC_INTERVAL = TimeSpan.FromMinutes(5);

        public CrowdDataSyncService(
            ApplicationDbContext context,
            CrowdDataApiService crowdApi,
            TransportationUploadService transportationUpload,
            ICacheService cache,
            ILogger<CrowdDataSyncService> logger)
        {
            _context = context;
            _crowdApi = crowdApi;
            _transportationUpload = transportationUpload;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 同步人流設備數據
        /// </summary>
        public async Task<CrowdDataSyncResponse> SyncCrowdDeviceDataAsync(bool forceExecution = false)
        {
            var response = new CrowdDataSyncResponse
            {
                Success = false,
                Message = "人流設備數據同步失敗"
            };

            try
            {
                _logger.LogInformation("開始同步人流設備數據");

                var devices = await _context.CrowdDevices
                    .Where(d => d.ApiUrl != null && d.ApiUrl.Contains("/a3dpc"))
                    .ToListAsync();

                var newRecords = new List<CrowdRecord>();
                var errors = new List<string>();
                var currentTime = DateTime.Now;

                foreach (var device in devices)
                {
                    if (!forceExecution && await ShouldSkipSync($"sync-a3dpc-{device.Serial}", currentTime))
                        continue;

                    try
                    {
                        var data = await _crowdApi.FetchCrowdDataAsync(device.ApiUrl!);
                        if (data == null || string.IsNullOrEmpty(data.Timestamp))
                            continue;

                        var dataTimestamp = DateTime.Parse(data.Timestamp);
                        var lastRecord = await GetLastCrowdRecord(device.Serial!);

                        if (dataTimestamp <= (lastRecord?.Time ?? DateTime.MinValue))
                            continue;

                        var (lastTimeIn, lastTimeOut) = GetLastCrowdCounts(lastRecord);
                        var newRecord = CreateCrowdRecord(device, data, currentTime, lastTimeIn, lastTimeOut);

                        // 只有當時間差超過5分鐘才記錄到 CrowdRecord
                        if (dataTimestamp - (lastRecord?.Time ?? DateTime.MinValue) > TimeSpan.FromMinutes(5))
                        {
                            newRecords.Add(newRecord);
                        }

                        await UpdateCrowdRecordLatest(newRecord);
                        await UpdateDeviceOnlineStatus(device, currentTime);
                        await _cache.SetAsync($"sync-a3dpc-{device.Serial}", currentTime, SYNC_INTERVAL);

                        // 上傳到交通部 API
                        await _transportationUpload.UploadCrowdDataAsync(newRecord.DeviceSerial, newRecord.Count);
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"同步人流設備數據失敗: {device.Serial} - {ex.Message}";
                        _logger.LogError(ex, errorMsg);
                        errors.Add(errorMsg);
                    }
                }

                if (newRecords.Any())
                {
                    _context.CrowdRecords.AddRange(newRecords);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("成功同步 {Count} 筆人流設備數據", newRecords.Count);
                }

                response.Success = true;
                response.Message = "人流設備數據同步已完成";
                response.SyncedRecords = newRecords.Count;
                response.FailedRecords = errors.Count;
                response.Errors = errors;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "同步人流設備數據時發生錯誤");
                response.Message = $"同步人流設備數據時發生錯誤: {ex.Message}";
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

        private async Task<CrowdRecord?> GetLastCrowdRecord(string deviceSerial)
        {
            return await _context.CrowdRecords
                .Where(r => r.DeviceSerial == deviceSerial)
                .OrderByDescending(r => r.Time)
                .FirstOrDefaultAsync();
        }

        private static (int lastTimeIn, int lastTimeOut) GetLastCrowdCounts(CrowdRecord? lastRecord)
        {
            if (lastRecord != null && lastRecord.Time > DateTime.Today)
            {
                return (lastRecord.TotalIn, lastRecord.TotalOut);
            }
            return (0, 0);
        }

        private static CrowdRecord CreateCrowdRecord(CrowdDevice device, CrowdApiResponse data, DateTime currentTime, int lastTimeIn, int lastTimeOut)
        {
            return new CrowdRecord
            {
                DeviceSerial = device.Serial ?? "",
                Time = currentTime,
                TotalIn = data.TotalIn,
                TotalOut = data.TotalOut,
                Count = data.Occupancy,
                In = data.TotalIn - lastTimeIn,
                Out = data.TotalOut - lastTimeOut,
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };
        }

        private async Task UpdateCrowdRecordLatest(CrowdRecord record)
        {
            var latest = new CrowdRecordLatest
            {
                DeviceSerial = record.DeviceSerial,
                Time = record.Time,
                TotalIn = record.TotalIn,
                TotalOut = record.TotalOut,
                Count = record.Count,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };

            _context.CrowdRecordLatests.Add(latest);
            await _context.SaveChangesAsync();

            // 刪除舊記錄
            var oldRecords = await _context.CrowdRecordLatests
                .Where(r => r.DeviceSerial == record.DeviceSerial && r.Id < latest.Id)
                .ToListAsync();

            _context.CrowdRecordLatests.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateDeviceOnlineStatus(CrowdDevice device, DateTime currentTime)
        {
            device.Status = "online";
            device.LatestOnlineTime = currentTime;

            await UpdateDeviceStatusLog("crowd", device.Serial ?? "");
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