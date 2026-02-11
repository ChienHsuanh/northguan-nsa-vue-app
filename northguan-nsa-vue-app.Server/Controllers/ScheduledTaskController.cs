using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Services.ScheduledTasks;

namespace northguan_nsa_vue_app.Server.Controllers
{
    /// <summary>
    /// 排程任務管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ScheduledTaskController : ControllerBase
    {
        private readonly IScheduledTaskService _scheduledTaskService;
        private readonly ILogger<ScheduledTaskController> _logger;

        public ScheduledTaskController(
            IScheduledTaskService scheduledTaskService,
            ILogger<ScheduledTaskController> logger)
        {
            _scheduledTaskService = scheduledTaskService;
            _logger = logger;
        }

        /// <summary>
        /// 手動執行設備在線狀態檢查
        /// </summary>
        [HttpPost("check-devices-online")]
        public async Task<ActionResult<DeviceOnlineCheckResponse>> CheckDevicesOnline()
        {
            try
            {
                var response = await _scheduledTaskService.CheckDevicesOnlineAsync(forceExecution: true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動執行設備在線狀態檢查失敗");
                var errorResponse = new DeviceOnlineCheckResponse
                {
                    Success = false,
                    Message = $"執行失敗: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// 手動執行人流設備數據同步
        /// </summary>
        [HttpPost("sync-crowd-data")]
        public async Task<ActionResult<CrowdDataSyncResponse>> SyncCrowdData()
        {
            try
            {
                var response = await _scheduledTaskService.SyncCrowdDeviceDataAsync(forceExecution: true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動執行人流設備數據同步失敗");
                var errorResponse = new CrowdDataSyncResponse
                {
                    Success = false,
                    Message = $"執行失敗: {ex.Message}",
                    Errors = new List<string> { ex.Message }
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// 手動執行停車記錄同步
        /// </summary>
        [HttpPost("sync-parking-data")]
        public async Task<ActionResult<ParkingDataSyncResponse>> SyncParkingData()
        {
            try
            {
                var response = await _scheduledTaskService.SyncParkingRecordAsync(forceExecution: true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動執行停車記錄同步失敗");
                var errorResponse = new ParkingDataSyncResponse
                {
                    Success = false,
                    Message = $"執行失敗: {ex.Message}",
                    Errors = new List<string> { ex.Message }
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// 手動執行交通設備數據同步
        /// </summary>
        [HttpPost("sync-traffic-data")]
        public async Task<ActionResult<TrafficDataSyncResponse>> SyncTrafficData()
        {
            try
            {
                var response = await _scheduledTaskService.SyncTrafficDeviceDataAsync(forceExecution: true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動執行交通設備數據同步失敗");
                var errorResponse = new TrafficDataSyncResponse
                {
                    Success = false,
                    Message = $"執行失敗: {ex.Message}",
                    Errors = new List<string> { ex.Message }
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// 手動執行資料庫備份
        /// </summary>
        [HttpPost("backup-database")]
        public async Task<ActionResult<DatabaseBackupResponse>> BackupDatabase()
        {
            try
            {
                var response = await _scheduledTaskService.BackupDatabaseAsync(forceExecution: true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動執行資料庫備份失敗");
                var errorResponse = new DatabaseBackupResponse
                {
                    Success = false,
                    Message = $"執行失敗: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// 手動執行日誌備份
        /// </summary>
        [HttpPost("backup-logs")]
        public async Task<ActionResult<LogBackupResponse>> BackupLogs()
        {
            try
            {
                var response = await _scheduledTaskService.BackupWarningLogAsync(forceExecution: true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動執行日誌備份失敗");
                var errorResponse = new LogBackupResponse
                {
                    Success = false,
                    Message = $"執行失敗: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// 手動執行審計日誌清理
        /// </summary>
        [HttpPost("cleanup-audit-logs")]
        public async Task<ActionResult<AuditLogCleanupResponse>> CleanupAuditLogs()
        {
            try
            {
                var response = await _scheduledTaskService.CheckAuditLogAsync(forceExecution: true);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "手動執行審計日誌清理失敗");
                var errorResponse = new AuditLogCleanupResponse
                {
                    Success = false,
                    Message = $"執行失敗: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// 獲取系統健康狀態
        /// </summary>
        [HttpGet("health")]
        public ActionResult<SystemHealthResponse> GetSystemHealth()
        {
            try
            {
                var healthResponse = new SystemHealthResponse
                {
                    IsHealthy = true,
                    CheckTime = DateTime.Now,
                    SystemInfo = new Dictionary<string, object>
                    {
                        ["ServerTime"] = DateTime.Now,
                        ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                        ["MachineName"] = Environment.MachineName,
                        ["ProcessorCount"] = Environment.ProcessorCount,
                        ["WorkingSet"] = Environment.WorkingSet,
                        ["Version"] = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"
                    }
                };

                return Ok(healthResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取系統健康狀態失敗");
                return StatusCode(500, new { message = "獲取系統健康狀態失敗", error = ex.Message });
            }
        }
    }
}