using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services.Infrastructure;
using System.Globalization;

namespace northguan_nsa_vue_app.Server.Controllers
{
    /// <summary>
    /// 接收外部系統（如 RtspToHttp）的告警
    /// </summary>
    [ApiController]
    [Route("api/external-alert")]
    public class ExternalAlertController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ExternalAlertController> _logger;

        public ExternalAlertController(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<ExternalAlertController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// 接收單一告警 (POST /api/external-alert/report)
        /// </summary>
        [HttpPost("report")]
        public async Task<ActionResult<ExternalAlertResponse>> Report([FromBody] ExternalAlertDto dto)
        {
            try
            {
                _logger.LogInformation("收到外部告警: Source={Source}, Camera={Camera}, Type={AlertType}, Status={Status}",
                    dto.Source, dto.Camera, dto.AlertType, dto.Status);

                // 用 Source + ":" + Camera 作為 Device 唯一 key
                var deviceSerial = $"{dto.Source}:{dto.Camera}";

                // 查找或建立 FenceDevice
                var device = await _context.FenceDevices
                    .Include(d => d.Station)
                    .FirstOrDefaultAsync(d => d.Serial == deviceSerial);

                if (device == null)
                {
                    // 查找或建立以 Source 命名的 Station
                    var station = await _context.Stations
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(s => s.Name == dto.Source && s.DeletedAt == null);

                    if (station == null)
                    {
                        station = new Station
                        {
                            Name = dto.Source,
                            EnableNotify = true
                        };
                        _context.Stations.Add(station);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("自動建立 Station: {Name} (Id={Id})", station.Name, station.Id);
                    }

                    device = new FenceDevice
                    {
                        Serial = deviceSerial,
                        Name = dto.Camera,
                        StationId = station.Id,
                        Status = "online"
                    };
                    _context.FenceDevices.Add(device);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("自動建立 FenceDevice: Serial={Serial}, Name={Name}", deviceSerial, dto.Camera);

                    // 重新載入含 Station 的完整實體
                    device = await _context.FenceDevices
                        .Include(d => d.Station)
                        .FirstAsync(d => d.Id == device.Id);
                }

                // 對應 Status → DeviceStatus
                var newStatus = dto.Status?.ToLowerInvariant() switch
                {
                    "error" => "offline",
                    "warning" => "warning",
                    "online" => "online",
                    _ => "online"
                };

                var previousStatus = device.Status;
                var statusChanged = previousStatus != newStatus;

                // 更新設備狀態
                device.Status = newStatus;
                device.LatestOnlineTime = DateTime.Now;
                device.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // 寫入 DeviceStatusLog（ErrorMessage = [AlertType] Message）
                var statusLog = new DeviceStatusLog
                {
                    DeviceType = "fence",
                    DeviceSerial = deviceSerial,
                    Status = newStatus,
                    Timestamp = DateTime.Now
                };
                _context.DeviceStatusLogs.Add(statusLog);
                await _context.SaveChangesAsync();

                // 狀態有變 → 發送 LINE 通知
                if (statusChanged)
                {
                    var station = device.Station;
                    if (station != null && !string.IsNullOrEmpty(station.LineToken) && station.EnableNotify)
                    {
                        var lineMessage = $"\n[{dto.AlertType}] {dto.Source}\n攝影機: {dto.Camera}\n狀態: {dto.Status}\n{dto.Message}\n時間: {dto.Timestamp}";
                        await _notificationService.SendLineNotificationAsync(station.LineToken, lineMessage);
                        _logger.LogInformation("已發送 LINE 告警通知: Station={Station}, Camera={Camera}", station.Name, dto.Camera);
                    }
                }

                return Ok(new ExternalAlertResponse { Message = "ok" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理外部告警時發生錯誤");
                return StatusCode(500, new ExternalAlertResponse { Message = ex.Message });
            }
        }
    }
}
