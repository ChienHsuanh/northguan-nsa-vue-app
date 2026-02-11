using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services.Infrastructure;

namespace northguan_nsa_vue_app.Server.Services.ScheduledTasks
{
    /// <summary>
    /// 設備在線狀態檢查服務
    /// </summary>
    public class DeviceOnlineCheckService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notification;
        private readonly ILogger<DeviceOnlineCheckService> _logger;

        // 設備離線時間閾值 (7分鐘)
        private static readonly TimeSpan DEVICE_OFFLINE_THRESHOLD = TimeSpan.FromMinutes(7);

        public DeviceOnlineCheckService(
            ApplicationDbContext context,
            INotificationService notification,
            ILogger<DeviceOnlineCheckService> logger)
        {
            _context = context;
            _notification = notification;
            _logger = logger;
        }

        public async Task<DeviceOnlineCheckResponse> CheckDevicesOnlineAsync(bool forceExecution = false)
        {
            var response = new DeviceOnlineCheckResponse
            {
                Success = false,
                Message = "設備在線狀態檢查失敗"
            };

            try
            {
                _logger.LogInformation("開始檢查設備在線狀態");

                var stations = await _context.Stations
                    .Include(s => s.CrowdDevices)
                    .Include(s => s.FenceDevices)
                    .Include(s => s.ParkingDevices)
                    .Include(s => s.TrafficDevices)
                    .Include(s => s.HighResolutionDevices)
                    .ToListAsync();

                var currentTime = DateTime.Now;
                var allOfflineDevices = new List<DeviceOfflineInfo>();
                int totalDevices = 0;
                int onlineDevices = 0;

                foreach (var station in stations)
                {
                    var offlineDevices = new List<DeviceOfflineInfo>();

                    // 檢查各類型設備並統計數量
                    totalDevices += await CheckDeviceTypeOnlineStatus(station.CrowdDevices, "crowd", currentTime, offlineDevices);
                    totalDevices += await CheckDeviceTypeOnlineStatus(station.FenceDevices, "fence", currentTime, offlineDevices);
                    totalDevices += await CheckDeviceTypeOnlineStatus(station.ParkingDevices, "parking", currentTime, offlineDevices);
                    totalDevices += await CheckTrafficDevicesOnlineStatus(station.TrafficDevices, currentTime, offlineDevices);
                    totalDevices += await CheckHighResolutionDevicesAlwaysOnline(station.HighResolutionDevices, currentTime);

                    allOfflineDevices.AddRange(offlineDevices);

                    // 發送離線通知
                    if (offlineDevices.Any() && station.EnableNotify && !string.IsNullOrEmpty(station.LineToken))
                    {
                        await _notification.SendDeviceOfflineNotificationAsync(station.Name, offlineDevices, station.LineToken);
                    }
                }

                onlineDevices = totalDevices - allOfflineDevices.Count;

                response.Success = true;
                response.Message = "設備在線狀態檢查已完成";
                response.TotalDevicesChecked = totalDevices;
                response.OnlineDevices = onlineDevices;
                response.OfflineDevices = allOfflineDevices.Count;
                response.OfflineDeviceList = allOfflineDevices;

                _logger.LogInformation("設備在線狀態檢查完成");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查設備在線狀態時發生錯誤");
                response.Message = $"檢查設備在線狀態時發生錯誤: {ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// 高解析度設備一律設為線上狀態
        /// </summary>
        private async Task<int> CheckHighResolutionDevicesAlwaysOnline<T>(
            IEnumerable<T> devices,
            DateTime currentTime) where T : class
        {
            int deviceCount = 0;

            foreach (var device in devices)
            {
                deviceCount++;

                // 設定最後上線時間為現在
                SetDeviceLatestOnlineTime(device, currentTime);

                // 設定狀態為線上
                SetDeviceStatus(device, "online");

                // 設定更新時間為現在
                SetDeviceUpdatedAt(device, currentTime);

                // 記錄狀態變更
                var serial = GetDeviceSerial(device);
                await UpdateDeviceStatusLog("highResolution", serial, "online");
            }

            await _context.SaveChangesAsync();
            return deviceCount;
        }

        /// <summary>
        /// 檢查車流設備在線狀態 - 排除有 ETag 代碼的設備
        /// </summary>
        private async Task<int> CheckTrafficDevicesOnlineStatus(
            IEnumerable<Models.TrafficDevice> devices,
            DateTime currentTime,
            List<DeviceOfflineInfo> offlineDevices)
        {
            int deviceCount = 0;

            foreach (var device in devices)
            {
                deviceCount++;

                // 如果設備有 ETag 代碼，跳過在線狀態檢查 (由 TrafficDataApiService 處理)
                if (!string.IsNullOrEmpty(device.ETagNumber))
                {
                    continue;
                }

                var latestOnlineTime = device.LatestOnlineTime;
                var status = device.Status;
                var serial = device.Serial;
                var name = device.Name;

                var isOnline = latestOnlineTime.HasValue &&
                              (currentTime - latestOnlineTime.Value) < DEVICE_OFFLINE_THRESHOLD;

                if (!isOnline && status != "offline")
                {
                    device.Status = "offline";
                    await UpdateDeviceStatusLog("traffic", serial);

                    offlineDevices.Add(new DeviceOfflineInfo
                    {
                        DeviceName = name,
                        DeviceSerial = serial,
                        DeviceType = "traffic",
                        LastOnlineTime = latestOnlineTime ?? DateTime.MinValue
                    });
                }
            }

            await _context.SaveChangesAsync();
            return deviceCount;
        }

        private async Task<int> CheckDeviceTypeOnlineStatus<T>(
            IEnumerable<T> devices,
            string deviceType,
            DateTime currentTime,
            List<DeviceOfflineInfo> offlineDevices) where T : class
        {
            int deviceCount = 0;

            foreach (var device in devices)
            {
                deviceCount++;
                var latestOnlineTime = GetDeviceLatestOnlineTime(device);
                var status = GetDeviceStatus(device);
                var serial = GetDeviceSerial(device);
                var name = GetDeviceName(device);

                var isOnline = latestOnlineTime.HasValue &&
                              (currentTime - latestOnlineTime.Value) < DEVICE_OFFLINE_THRESHOLD;

                if (!isOnline && status != "offline")
                {
                    SetDeviceStatus(device, "offline");
                    await UpdateDeviceStatusLog(deviceType, serial);

                    offlineDevices.Add(new DeviceOfflineInfo
                    {
                        DeviceName = name,
                        DeviceSerial = serial,
                        DeviceType = deviceType,
                        LastOnlineTime = latestOnlineTime ?? DateTime.MinValue
                    });
                }
            }

            await _context.SaveChangesAsync();
            return deviceCount;
        }

        private async Task UpdateDeviceStatusLog(string deviceType, string serial, string status = "offline")
        {
            var statusLog = new DeviceStatusLog
            {
                DeviceType = deviceType,
                DeviceSerial = serial,
                Status = status,
                Timestamp = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.DeviceStatusLogs.Add(statusLog);
            await _context.SaveChangesAsync();
        }

        // 反射方法來處理不同類型的設備
        private static DateTime? GetDeviceLatestOnlineTime(object device)
        {
            var property = device.GetType().GetProperty("LatestOnlineTime");
            return property?.GetValue(device) as DateTime?;
        }

        private static string GetDeviceStatus(object device)
        {
            var property = device.GetType().GetProperty("Status");
            return property?.GetValue(device) as string ?? "unknown";
        }

        private static string GetDeviceSerial(object device)
        {
            var property = device.GetType().GetProperty("Serial");
            return property?.GetValue(device) as string ?? "";
        }

        private static string GetDeviceName(object device)
        {
            var property = device.GetType().GetProperty("Name");
            return property?.GetValue(device) as string ?? "";
        }

        private static void SetDeviceStatus(object device, string status)
        {
            var property = device.GetType().GetProperty("Status");
            property?.SetValue(device, status);
        }

        private static void SetDeviceLatestOnlineTime(object device, DateTime onlineTime)
        {
            var property = device.GetType().GetProperty("LatestOnlineTime");
            property?.SetValue(device, onlineTime);
        }

        private static void SetDeviceUpdatedAt(object device, DateTime updatedTime)
        {
            var property = device.GetType().GetProperty("UpdatedAt");
            property?.SetValue(device, updatedTime);
        }
    }
}