using Namotion.Reflection;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Services.Infrastructure;
using System.Net;
using System.Text.Json;

namespace northguan_nsa_vue_app.Server.Services.ExternalApi
{
    /// <summary>
    /// 人流數據 API 服務 (A3DPC)
    /// </summary>
    public class CrowdDataApiService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CrowdDataApiService> _logger;

        public CrowdDataApiService(
            IConfiguration configuration,
            ILogger<CrowdDataApiService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 獲取人流數據 (A3DPC API)
        /// </summary>
        public async Task<CrowdApiResponse?> FetchCrowdDataAsync(string apiUrl, string username = "", string password = "")
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    username = _configuration.GetValue<string>("ExternalApi:CrowdDevice:username") ?? "user";
                if (string.IsNullOrEmpty(password))
                    password = _configuration.GetValue<string>("ExternalApi:CrowdDevice:password") ?? "p@ssw0rd";

                _logger.LogDebug("正在獲取人流數據: {ApiUrl}", apiUrl);

                var separator = apiUrl.EndsWith("/") ? "" : "/";
                var url = $"{apiUrl}{separator}api/occupancy";

                // 創建支援 Digest 認證的 HttpClient
                var handler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(username, password)
                };

                using var httpClient = new HttpClient(handler);
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("獲取人流數據失敗，狀態碼: {StatusCode}, URL: {Url}",
                        response.StatusCode, url);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CrowdApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogDebug("成功獲取人流數據: {Data}", content);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取人流數據時發生錯誤: {ApiUrl}", apiUrl);
                return null;
            }
        }
    }
}