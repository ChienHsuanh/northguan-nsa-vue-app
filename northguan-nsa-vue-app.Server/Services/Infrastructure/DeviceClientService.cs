using northguan_nsa_vue_app.Server.DTOs;
using System.Text.Json;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    public class DeviceClientService : IDeviceClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeviceClientService> _logger;

        public DeviceClientService(HttpClient httpClient, ILogger<DeviceClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // 設置 HTTP 客戶端超時時間
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<CrowdDeviceDataResponse?> FetchCrowdDataAsync(string apiUrl)
        {
            try
            {
                _logger.LogDebug("正在從 {ApiUrl} 獲取人流數據", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("獲取人流數據失敗，狀態碼: {StatusCode}, URL: {ApiUrl}", 
                        response.StatusCode, apiUrl);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("從 {ApiUrl} 獲取的人流數據為空", apiUrl);
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var data = JsonSerializer.Deserialize<CrowdDeviceDataResponse>(content, options);
                
                _logger.LogDebug("成功從 {ApiUrl} 獲取人流數據", apiUrl);
                return data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP 請求錯誤，無法從 {ApiUrl} 獲取人流數據", apiUrl);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "請求超時，無法從 {ApiUrl} 獲取人流數據", apiUrl);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON 解析錯誤，無法解析從 {ApiUrl} 獲取的人流數據", apiUrl);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取人流數據時發生未知錯誤，URL: {ApiUrl}", apiUrl);
                return null;
            }
        }

        public async Task<ParkingDeviceDataResponse?> FetchParkingDataAsync(string apiUrl)
        {
            try
            {
                _logger.LogDebug("正在從 {ApiUrl} 獲取停車數據", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("獲取停車數據失敗，狀態碼: {StatusCode}, URL: {ApiUrl}", 
                        response.StatusCode, apiUrl);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("從 {ApiUrl} 獲取的停車數據為空", apiUrl);
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var data = JsonSerializer.Deserialize<ParkingDeviceDataResponse>(content, options);
                
                _logger.LogDebug("成功從 {ApiUrl} 獲取停車數據", apiUrl);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取停車數據時發生錯誤，URL: {ApiUrl}", apiUrl);
                return null;
            }
        }

        public async Task<TrafficDeviceDataResponse?> FetchTrafficDataAsync(string apiUrl)
        {
            try
            {
                _logger.LogDebug("正在從 {ApiUrl} 獲取交通數據", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("獲取交通數據失敗，狀態碼: {StatusCode}, URL: {ApiUrl}", 
                        response.StatusCode, apiUrl);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("從 {ApiUrl} 獲取的交通數據為空", apiUrl);
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var data = JsonSerializer.Deserialize<TrafficDeviceDataResponse>(content, options);
                
                _logger.LogDebug("成功從 {ApiUrl} 獲取交通數據", apiUrl);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取交通數據時發生錯誤，URL: {ApiUrl}", apiUrl);
                return null;
            }
        }

        public async Task<CvpDeviceDataResponse?> FetchCvpDataAsync(string apiUrl)
        {
            try
            {
                _logger.LogDebug("正在從 {ApiUrl} 獲取 CVP 數據", apiUrl);

                var response = await _httpClient.GetAsync(apiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("獲取 CVP 數據失敗，狀態碼: {StatusCode}, URL: {ApiUrl}", 
                        response.StatusCode, apiUrl);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("從 {ApiUrl} 獲取的 CVP 數據為空", apiUrl);
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var data = JsonSerializer.Deserialize<CvpDeviceDataResponse>(content, options);
                
                _logger.LogDebug("成功從 {ApiUrl} 獲取 CVP 數據", apiUrl);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取 CVP 數據時發生錯誤，URL: {ApiUrl}", apiUrl);
                return null;
            }
        }
    }
}