using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services.ExternalApi;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 交通數據同步服務
    /// </summary>
    public class TrafficDataSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly TrafficDataApiService _trafficApi;
        private readonly ICacheService _cache;
        private readonly ILogger<TrafficDataSyncService> _logger;
        private readonly IConfiguration _configuration;

        // 配置快取
        private TrafficSyncConfig? _config;
        private DateTime _configLastLoaded = DateTime.MinValue;
        private static readonly TimeSpan CONFIG_CACHE_DURATION = TimeSpan.FromMinutes(5);


        public TrafficDataSyncService(
            ApplicationDbContext context,
            TrafficDataApiService trafficApi,
            ICacheService cache,
            ILogger<TrafficDataSyncService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _trafficApi = trafficApi;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 同步交通設備數據 - 按城市分組以減少 API 調用
        /// </summary>
        public async Task<TrafficDataSyncResponse> SyncTrafficDeviceDataAsync(bool forceExecution = false)
        {
            var response = new TrafficDataSyncResponse
            {
                Success = false,
                Message = "交通設備數據同步失敗"
            };

            try
            {
                _logger.LogInformation("開始同步交通設備數據");

                var devices = await _context.TrafficDevices
                    .Where(d => !string.IsNullOrEmpty(d.ETagNumber) && !string.IsNullOrEmpty(d.City))
                    .ToListAsync();

                var newRecords = new List<TrafficRecord>();
                var errors = new List<string>();
                var skippedCount = 0;
                var currentTime = DateTime.Now;

                var config = await GetConfigAsync();

                // 按城市分組設備
                var devicesByCity = devices.GroupBy(d => d.City).ToList();
                _logger.LogInformation("發現 {CityCount} 個城市的 {DeviceCount} 個交通設備", 
                    devicesByCity.Count, devices.Count);

                foreach (var cityGroup in devicesByCity)
                {
                    var city = cityGroup.Key;
                    var cityDevices = cityGroup.ToList();

                    try
                    {
                        // 檢查城市級別的同步間隔
                        var cityCacheKey = $"sync-traffic-city-{city}";
                        if (!forceExecution && await ShouldSkipSync(cityCacheKey, currentTime, config))
                        {
                            skippedCount += cityDevices.Count;
                            if (config.EnableVerboseLogging)
                            {
                                _logger.LogDebug("跳過城市 {City} 的 {DeviceCount} 個設備：尚未到達同步間隔時間", 
                                    city, cityDevices.Count);
                            }
                            continue;
                        }

                        _logger.LogInformation("開始同步城市 {City} 的 {DeviceCount} 個設備", city, cityDevices.Count);

                        // 一次性獲取整個城市的數據
                        var cityData = await _trafficApi.FetchCityTrafficDataAsync(city);
                        if (cityData?.ETagPairLives?.Any() != true)
                        {
                            skippedCount += cityDevices.Count;
                            _logger.LogWarning("城市 {City} 無可用的交通數據", city);
                            continue;
                        }

                        // 處理該城市的每個設備
                        foreach (var device in cityDevices)
                        {
                            try
                            {
                                // 從城市數據中找到匹配的 ETag 數據
                                var matchingPair = cityData.ETagPairLives
                                    .FirstOrDefault(pair => pair.ETagPairID == device.ETagNumber);

                                if (matchingPair == null)
                                {
                                    skippedCount++;
                                    if (config.EnableVerboseLogging)
                                    {
                                        _logger.LogDebug("城市 {City} 數據中未找到設備 {Serial} 的 ETag {ETagNumber}", 
                                            city, device.Serial, device.ETagNumber);
                                    }
                                    continue;
                                }

                                // 處理 ETag 數據
                                var trafficData = ProcessETagPairData(matchingPair);
                                if (trafficData == null)
                                {
                                    skippedCount++;
                                    if (config.EnableVerboseLogging)
                                    {
                                        _logger.LogDebug("設備 {Serial} 的 ETag 數據無效", device.Serial);
                                    }
                                    continue;
                                }

                                var newRecord = CreateTrafficRecord(device, trafficData, currentTime);
                                newRecords.Add(newRecord);

                                await UpdateDeviceOnlineStatus(device, currentTime);
                            }
                            catch (Exception ex)
                            {
                                var errorMsg = $"處理設備數據失敗: {device.Serial} - {ex.Message}";
                                _logger.LogError(ex, errorMsg);
                                errors.Add(errorMsg);
                            }
                        }

                        // 更新城市級別的同步時間
                        await _cache.SetAsync(cityCacheKey, currentTime, TimeSpan.FromMinutes(config.SyncIntervalMinutes));

                        _logger.LogInformation("完成城市 {City} 的數據同步", city);
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"同步城市 {city} 數據失敗: {ex.Message}";
                        _logger.LogError(ex, errorMsg);
                        errors.Add(errorMsg);
                        skippedCount += cityDevices.Count;
                    }

                    // 城市間延遲
                    if (devicesByCity.Count > 1 && config.BatchDelaySeconds > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(config.BatchDelaySeconds));
                    }
                }

                if (newRecords.Any())
                {
                    _context.TrafficRecords.AddRange(newRecords);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("成功同步 {Count} 筆交通記錄", newRecords.Count);
                }

                response.Success = true;
                response.Message = $"交通設備數據同步已完成 - 成功: {newRecords.Count}, 失敗: {errors.Count}, 跳過: {skippedCount}";
                response.SyncedRecords = newRecords.Count;
                response.FailedRecords = errors.Count;
                response.SkippedRecords = skippedCount;
                response.Errors = errors;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "同步交通設備數據時發生錯誤");
                response.Message = $"同步交通設備數據時發生錯誤: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        #region Private Methods

        /// <summary>
        /// 獲取配置
        /// </summary>
        private async Task<TrafficSyncConfig> GetConfigAsync()
        {
            if (_config == null || DateTime.Now - _configLastLoaded > CONFIG_CACHE_DURATION)
            {
                _config = new TrafficSyncConfig();
                _configuration.GetSection("TrafficSync").Bind(_config);
                _configLastLoaded = DateTime.Now;

                if (_config.EnableVerboseLogging)
                {
                    _logger.LogInformation("載入交通同步配置: {@Config}", _config);
                }
            }

            return _config;
        }

        private async Task<bool> ShouldSkipSync(string cacheKey, DateTime currentTime, TrafficSyncConfig config)
        {
            if (!await _cache.ExistsAsync(cacheKey))
                return false;

            var lastExec = await _cache.GetAsync<DateTime?>(cacheKey);
            var syncInterval = TimeSpan.FromMinutes(config.SyncIntervalMinutes);
            return lastExec.HasValue && currentTime - lastExec.Value < syncInterval;
        }

        /// <summary>
        /// 處理 ETag 配對數據，轉換為 TrafficApiResponse
        /// </summary>
        private TrafficApiResponse? ProcessETagPairData(DTOs.ETagPairLive eTagPair)
        {
            try
            {
                if (!DateTime.TryParse(eTagPair.DataCollectTime, out var collectTime))
                {
                    _logger.LogDebug("ETag {ETagPairID} 數據收集時間無效: {DataCollectTime}", 
                        eTagPair.ETagPairID, eTagPair.DataCollectTime);
                    return null;
                }

                // 尋找車輛類型為 3 的數據
                var targetFlow = eTagPair.Flows?.FirstOrDefault(f => f.VehicleType == 3);
                if (targetFlow == null)
                {
                    _logger.LogDebug("ETag {ETagPairID} 沒有車輛類型 3 的數據", eTagPair.ETagPairID);
                    return null;
                }

                return new TrafficApiResponse
                {
                    TravelTime = targetFlow.TravelTime,
                    SpaceMeanSpeed = targetFlow.SpaceMeanSpeed > 0 ? targetFlow.SpaceMeanSpeed : 0,
                    DataCollectTime = collectTime,
                    VehicleCount = targetFlow.VehicleCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理 ETag {ETagPairID} 數據時發生錯誤", eTagPair.ETagPairID);
                return null;
            }
        }




        private static TrafficRecord CreateTrafficRecord(TrafficDevice device, TrafficApiResponse data, DateTime currentTime)
        {
            return new TrafficRecord
            {
                DeviceSerial = device.Serial ?? "",
                Time = data.DataCollectTime,
                VehicleCount = data.VehicleCount,
                AverageSpeed = (decimal)data.SpaceMeanSpeed,
                TravelTime = (decimal)data.TravelTime,  // 添加 TravelTime 欄位
                TrafficCondition = data.SpaceMeanSpeed > 60 ? "smooth" : data.SpaceMeanSpeed > 30 ? "normal" : "congested",
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };
        }

        private async Task UpdateDeviceOnlineStatus(TrafficDevice device, DateTime currentTime)
        {
            device.Status = "online";
            device.LatestOnlineTime = currentTime;

            await UpdateDeviceStatusLog("traffic", device.Serial ?? "");
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