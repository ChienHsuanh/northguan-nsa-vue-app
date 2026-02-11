using northguan_nsa_vue_app.Server.DTOs;
using System.Text;
using System.Text.Json;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;
        private const string LINE_NOTIFY_API_URL = "https://notify-api.line.me/api/notify";

        public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendLineNotificationAsync(string token, string message)
        {
            await SendLineNotificationAsync(token, message, null);
        }

        public async Task SendLineNotificationAsync(string token, string message, string? imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogWarning("LINE 通知參數無效：token 或 message 為空");
                    return;
                }

                _logger.LogDebug("正在發送 LINE 通知：{Message}", message);

                var request = new HttpRequestMessage(HttpMethod.Post, LINE_NOTIFY_API_URL);
                request.Headers.Add("Authorization", $"Bearer {token}");

                var formData = new List<KeyValuePair<string, string>>
                {
                    new("message", message)
                };

                // 如果有圖片 URL，加入到表單資料中
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    formData.Add(new("imageThumbnail", imageUrl));
                    formData.Add(new("imageFullsize", imageUrl));
                }

                request.Content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("LINE 通知發送成功");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("LINE 通知發送失敗，狀態碼：{StatusCode}，錯誤內容：{ErrorContent}", 
                        response.StatusCode, errorContent);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "發送 LINE 通知時發生 HTTP 錯誤");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "發送 LINE 通知時請求超時");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送 LINE 通知時發生未知錯誤");
            }
        }

        public async Task SendDeviceOfflineNotificationAsync(string stationName, List<DeviceOfflineInfo> offlineDevices, string? lineToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lineToken) || !offlineDevices.Any())
                {
                    return;
                }

                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"{stationName} - 已離線超過7分鐘");

                foreach (var device in offlineDevices)
                {
                    messageBuilder.AppendLine($"{device.DeviceName} 裝置編號({device.DeviceSerial})");
                }

                var message = messageBuilder.ToString();
                await SendLineNotificationAsync(lineToken, message);

                _logger.LogInformation("已發送設備離線通知給站點：{StationName}，離線設備數量：{Count}", 
                    stationName, offlineDevices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送設備離線通知時發生錯誤，站點：{StationName}", stationName);
            }
        }
    }
}