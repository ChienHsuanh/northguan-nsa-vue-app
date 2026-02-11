using northguan_nsa_vue_app.Server.DTOs;
using System.Text;
using System.Text.Json;

namespace northguan_nsa_vue_app.Server.Services.ExternalApi
{
    /// <summary>
    /// 交通部數據上傳服務
    /// </summary>
    public class TransportationUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransportationUploadService> _logger;

        private readonly string _uploadUrl;
        private readonly string _apiKey;
        private readonly bool _enabled;

        public TransportationUploadService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TransportationUploadService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _uploadUrl = _configuration["ExternalApi:Transportation:UploadUrl"] ?? "";
            _apiKey = _configuration["ExternalApi:Transportation:ApiKey"] ?? "";
            _enabled = _configuration.GetValue<bool>("ExternalApi:Transportation:Enabled", false);
        }

        /// <summary>
        /// 上傳數據到交通部 API
        /// </summary>
        public async Task<bool> UploadTransportationDataAsync(TransportationUploadRequest request)
        {
            try
            {
                if (!_enabled)
                {
                    _logger.LogDebug("交通部數據上傳功能已停用");
                    return true; // 返回 true 避免影響主要流程
                }

                if (string.IsNullOrEmpty(_uploadUrl) || string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("交通部 API 配置不完整");
                    return false;
                }

                _logger.LogDebug("正在上傳交通部數據: {DataType}, {DeviceSerial}", request.DataType, request.DeviceSerial);

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, _uploadUrl)
                {
                    Content = content
                };

                // 添加 API Key 到標頭
                httpRequest.Headers.Add("X-API-Key", _apiKey);

                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("交通部數據上傳成功: {DataType}, {DeviceSerial}", request.DataType, request.DeviceSerial);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("交通部數據上傳失敗，狀態碼: {StatusCode}, 錯誤: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳交通部數據時發生錯誤: {DataType}, {DeviceSerial}", request.DataType, request.DeviceSerial);
                return false;
            }
        }

        /// <summary>
        /// 上傳人流數據到交通部
        /// </summary>
        public async Task<bool> UploadCrowdDataAsync(string deviceSerial, int count)
        {
            var uploadData = new CrowdUploadData
            {
                Serial = deviceSerial,
                Count = count
            };

            var request = new TransportationUploadRequest
            {
                DataType = "crowd",
                DeviceSerial = deviceSerial,
                Data = uploadData
            };

            return await UploadTransportationDataAsync(request);
        }

        /// <summary>
        /// 上傳停車數據到交通部
        /// </summary>
        public async Task<bool> UploadParkingDataAsync(string deviceSerial, int remainingNum, int admittanceNum)
        {
            var uploadData = new ParkingUploadData
            {
                Serial = deviceSerial,
                RemainingNum = remainingNum,
                AdmittanceNum = admittanceNum
            };

            var request = new TransportationUploadRequest
            {
                DataType = "parking",
                DeviceSerial = deviceSerial,
                Data = uploadData
            };

            return await UploadTransportationDataAsync(request);
        }
    }
}