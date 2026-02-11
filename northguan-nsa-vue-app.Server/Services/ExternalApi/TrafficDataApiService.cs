using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ExternalApi
{
    /// <summary>
    /// 交通數據 API 服務 (TDX)
    /// </summary>
    public class TrafficDataApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICacheService _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TrafficDataApiService> _logger;
        private readonly ApplicationDbContext _context;

        private readonly string _tdxClientId;
        private readonly string _tdxClientSecret;

        // TDX 設備離線時間閾值 (70分鐘，因為 TDX API 每小時調用一次)
        private static readonly TimeSpan TDX_DEVICE_OFFLINE_THRESHOLD = TimeSpan.FromMinutes(70);

        public TrafficDataApiService(
            IHttpClientFactory httpClientFactory,
            ICacheService cache,
            IConfiguration configuration,
            ILogger<TrafficDataApiService> logger,
            ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
            _context = context;

            _tdxClientId = _configuration["ExternalApi:TDX:ClientId"] ?? throw new ArgumentException("TDX ClientId 未設定");
            _tdxClientSecret = _configuration["ExternalApi:TDX:ClientSecret"] ?? throw new ArgumentException("TDX ClientSecret 未設定");
        }

        /// <summary>
        /// 獲取交通數據 (TDX API) 並更新設備狀態
        /// </summary>
        public async Task<TrafficApiResponse?> FetchTrafficDataAsync(string eTagNumber, string city)
        {
            try
            {
                _logger.LogDebug("正在獲取交通數據: {ETagNumber}, 城市: {City}", eTagNumber, city);

                var token = await GetTdxTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("無法獲取 TDX Token");
                    return null;
                }

                var url = $"https://tdx.transportdata.tw/api/basic/v2/Road/Traffic/Live/ETag/City/{city}?top=1000&format=JSON";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var httpClient = _httpClientFactory.CreateClient("TdxApi");
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("TDX API 速率限制，狀態碼: {StatusCode}", response.StatusCode);
                        throw new HttpRequestException($"TooManyRequests: {response.StatusCode}");
                    }

                    _logger.LogWarning("獲取交通數據失敗，狀態碼: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tdxResponse = JsonSerializer.Deserialize<TdxETagResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tdxResponse?.ETagPairLives?.Any() != true)
                {
                    _logger.LogWarning("TDX 響應中沒有數據，城市: {City}", city);
                    return null;
                }

                // 尋找匹配的 ETag 配對路段
                var matchingPair = tdxResponse.ETagPairLives
                    .FirstOrDefault(pair => pair.ETagPairID == eTagNumber);

                if (matchingPair == null)
                {
                    _logger.LogWarning("在城市 {City} 的交通數據中未找到 ETag {ETagNumber}", city, eTagNumber);
                    return null;
                }

                // 處理匹配的 ETag 數據，找到車輛類型為 3 的數據
                if (DateTime.TryParse(matchingPair.DataCollectTime, out var collectTime))
                {
                    foreach (var flow in matchingPair.Flows)
                    {
                        if (flow.VehicleType == 3) // 車輛類型 3
                        {
                            // 更新設備在線狀態
                            await UpdateTdxDeviceStatusAsync(eTagNumber, collectTime);

                            return new TrafficApiResponse
                            {
                                TravelTime = flow.TravelTime,
                                SpaceMeanSpeed = flow.SpaceMeanSpeed > 0 ? flow.SpaceMeanSpeed : 0,
                                DataCollectTime = collectTime,
                                VehicleCount = flow.VehicleCount
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取交通數據時發生錯誤: {ETagNumber}", eTagNumber);
                return null;
            }
        }

        // 用於防止併發請求同時獲取 token 的鎖
        private static readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        /// <summary>
        /// 獲取 TDX Token
        /// </summary>
        public async Task<string?> GetTdxTokenAsync()
        {
            const string cacheKey = "tdx_token";

            try
            {
                // 首次檢查快取（無鎖）
                var cachedToken = await _cache.GetAsync<string>(cacheKey);
                if (!string.IsNullOrEmpty(cachedToken))
                {
                    _logger.LogDebug("快取命中，返回已快取的 TDX Token");
                    return cachedToken;
                }

                // 使用信號量防止併發請求
                await _tokenSemaphore.WaitAsync();
                try
                {
                    // 雙重檢查快取（在鎖內）
                    cachedToken = await _cache.GetAsync<string>(cacheKey);
                    if (!string.IsNullOrEmpty(cachedToken))
                    {
                        _logger.LogDebug("鎖內快取命中，返回已快取的 TDX Token");
                        return cachedToken;
                    }

                    _logger.LogDebug("快取未命中，正在獲取新的 TDX Token");

                    var tokenResponse = await GetTokenWithRetry();
                    if (tokenResponse == null)
                    {
                        return null;
                    }

                    // 快取 Token，提前 5 分鐘過期，但至少保留 5 分鐘
                    var cacheExpiry = TimeSpan.FromSeconds(Math.Max(tokenResponse.ExpiresIn - 300, 300));
                    await _cache.SetAsync(cacheKey, tokenResponse.AccessToken, cacheExpiry);

                    _logger.LogInformation("成功獲取並快取 TDX Token，過期時間: {Expiry} 秒", cacheExpiry.TotalSeconds);
                    return tokenResponse.AccessToken;
                }
                finally
                {
                    _tokenSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 TDX Token 時發生錯誤");
                return null;
            }
        }

        /// <summary>
        /// 獲取 TDX Token（簡化版本，由 Polly 處理重試）
        /// </summary>
        private async Task<TdxTokenResponse?> GetTokenWithRetry()
        {
            try
            {
                var url = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _tdxClientId),
                    new KeyValuePair<string, string>("client_secret", _tdxClientSecret)
                });

                using var httpClient = _httpClientFactory.CreateClient("TdxApi");
                var response = await httpClient.PostAsync(url, formData);

                // Polly 會自動處理重試
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TdxTokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
                {
                    _logger.LogError("TDX Token 響應無效，AccessToken 為空");
                    return null;
                }

                if (tokenResponse.ExpiresIn <= 0)
                {
                    _logger.LogWarning("TDX Token ExpiresIn 值異常: {ExpiresIn}，使用預設值 3600 秒", tokenResponse.ExpiresIn);
                    tokenResponse.ExpiresIn = 3600; // 預設 1 小時
                }

                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 TDX Token 失敗");
                throw;
            }
        }

        /// <summary>
        /// 更新 TDX 車流設備狀態
        /// </summary>
        private async Task UpdateTdxDeviceStatusAsync(string eTagNumber, DateTime dataCollectTime)
        {
            try
            {
                var device = await _context.TrafficDevices
                    .FirstOrDefaultAsync(d => d.ETagNumber == eTagNumber);

                if (device != null)
                {
                    device.LatestOnlineTime = dataCollectTime;
                    device.Status = "online";
                    device.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    _logger.LogDebug("已更新 TDX 設備狀態: {ETagNumber}, 最新在線時間: {OnlineTime}", eTagNumber, dataCollectTime);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新 TDX 設備狀態時發生錯誤: {ETagNumber}", eTagNumber);
            }
        }

        /// <summary>
        /// 獲取整個城市的交通數據 (供 TrafficDataSyncService 使用)
        /// </summary>
        public async Task<TdxETagResponse?> FetchCityTrafficDataAsync(string city)
        {
            try
            {
                _logger.LogDebug("正在獲取城市交通數據: {City}", city);

                var token = await GetTdxTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("無法獲取 TDX Token");
                    return null;
                }

                var url = $"https://tdx.transportdata.tw/api/basic/v2/Road/Traffic/Live/ETag/City/{city}?top=1000&format=JSON";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var httpClient = _httpClientFactory.CreateClient("TdxApi");
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("TDX API 速率限制，狀態碼: {StatusCode}", response.StatusCode);
                        throw new HttpRequestException($"TooManyRequests: {response.StatusCode}");
                    }

                    _logger.LogWarning("獲取城市交通數據失敗，狀態碼: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tdxResponse = JsonSerializer.Deserialize<TdxETagResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tdxResponse?.ETagPairLives?.Any() != true)
                {
                    _logger.LogWarning("城市 {City} 沒有可用的交通數據", city);
                    return null;
                }

                _logger.LogDebug("成功獲取城市 {City} 的 {Count} 個 ETag 路段數據", city, tdxResponse.ETagPairLives.Count);
                return tdxResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取城市交通數據時發生錯誤: {City}", city);
                return null;
            }
        }

        /// <summary>
        /// 檢查所有 TDX 車流設備的在線狀態
        /// </summary>
        public async Task<TdxDeviceStatusCheckResponse> CheckTdxDevicesStatusAsync()
        {
            var response = new TdxDeviceStatusCheckResponse
            {
                Success = false,
                Message = "TDX 設備狀態檢查失敗"
            };

            try
            {
                _logger.LogInformation("開始檢查 TDX 車流設備狀態");

                var tdxDevices = await _context.TrafficDevices
                    .Where(d => !string.IsNullOrEmpty(d.ETagNumber))
                    .Include(d => d.Station)
                    .ToListAsync();

                var currentTime = DateTime.Now;
                var offlineDevices = new List<DeviceOfflineInfo>();
                int totalTdxDevices = tdxDevices.Count;

                foreach (var device in tdxDevices)
                {
                    var isOnline = device.LatestOnlineTime.HasValue &&
                                  (currentTime - device.LatestOnlineTime.Value) < TDX_DEVICE_OFFLINE_THRESHOLD;

                    if (!isOnline && device.Status != "offline")
                    {
                        device.Status = "offline";
                        device.UpdatedAt = DateTime.Now;

                        // 記錄設備狀態日誌
                        var statusLog = new DeviceStatusLog
                        {
                            DeviceType = "traffic",
                            DeviceSerial = device.Serial,
                            Status = "offline",
                            Timestamp = DateTime.Now,
                            CreatedAt = DateTime.Now
                        };
                        _context.DeviceStatusLogs.Add(statusLog);

                        offlineDevices.Add(new DeviceOfflineInfo
                        {
                            DeviceName = device.Name,
                            DeviceSerial = device.Serial,
                            DeviceType = "traffic",
                            LastOnlineTime = device.LatestOnlineTime ?? DateTime.MinValue
                        });

                        _logger.LogWarning("TDX 設備離線: {DeviceName} ({ETagNumber}), 最後在線時間: {LastOnlineTime}",
                            device.Name, device.ETagNumber, device.LatestOnlineTime);
                    }
                }

                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "TDX 設備狀態檢查完成";
                response.TotalTdxDevices = totalTdxDevices;
                response.OnlineTdxDevices = totalTdxDevices - offlineDevices.Count;
                response.OfflineTdxDevices = offlineDevices.Count;
                response.OfflineDeviceList = offlineDevices;

                _logger.LogInformation("TDX 設備狀態檢查完成: 總計 {Total}, 在線 {Online}, 離線 {Offline}",
                    totalTdxDevices, response.OnlineTdxDevices, response.OfflineTdxDevices);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查 TDX 設備狀態時發生錯誤");
                response.Message = $"檢查 TDX 設備狀態時發生錯誤: {ex.Message}";
                return response;
            }
        }
    }
}