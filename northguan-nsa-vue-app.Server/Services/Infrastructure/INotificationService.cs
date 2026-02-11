using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Services.Infrastructure
{
    /// <summary>
    /// 通知服務接口
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 發送 LINE 通知
        /// </summary>
        Task SendLineNotificationAsync(string token, string message);

        /// <summary>
        /// 發送 LINE 通知（含圖片）
        /// </summary>
        Task SendLineNotificationAsync(string token, string message, string? imageUrl);

        /// <summary>
        /// 發送設備離線通知
        /// </summary>
        Task SendDeviceOfflineNotificationAsync(string stationName, List<DeviceOfflineInfo> offlineDevices, string? lineToken);
    }
}