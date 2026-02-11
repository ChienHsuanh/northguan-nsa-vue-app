using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using northguan_nsa_vue_app.Server.Common;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.Services.Monitoring;
using northguan_nsa_vue_app.Server.Utils;

namespace northguan_nsa_vue_app.Server.Controllers.Record
{
    /// <summary>
    /// 停車記錄控制器 - 重構版本
    /// 提供完整的錯誤處理、驗證和監控
    /// </summary>
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ParkingController(
        IParkingRecordService parkingRecordService,
        IAuthService authService,
        IPerformanceMonitor performanceMonitor,
        ILogger<ParkingController> logger) : ControllerBase
    {
        private readonly IParkingRecordService _parkingRecordService = parkingRecordService;
        private readonly IAuthService _authService = authService;
        private readonly IPerformanceMonitor _performanceMonitor = performanceMonitor;
        private readonly ILogger<ParkingController> _logger = logger;

        /// <summary>
        /// 獲取停車記錄列表
        /// 支持高級過濾：使用率範圍、總車位數範圍、日期範圍等
        /// </summary>
        /// <param name="parameters">查詢參數</param>
        /// <returns>分頁的停車記錄列表</returns>
        [HttpGet("parking-records")]
        [ProducesResponseType(typeof(Result<PagedResponse<ParkingRecordListResponse>>), 200)]
        [ProducesResponseType(typeof(Result<PagedResponse<ParkingRecordListResponse>>), 400)]
        [ProducesResponseType(typeof(Result<PagedResponse<ParkingRecordListResponse>>), 403)]
        [ProducesResponseType(typeof(Result<PagedResponse<ParkingRecordListResponse>>), 500)]
        public async Task<ActionResult<Result<PagedResponse<ParkingRecordListResponse>>>> GetRecordsList([FromQuery] ParkingRecordQueryParameters parameters)
        {
            using var operation = _performanceMonitor.StartOperation("GetParkingRecords",
                new Dictionary<string, object>
                {
                    ["UserId"] = _authService.GetCurrentUserId(),
                    ["Parameters"] = parameters
                });

            try
            {
                // FluentValidation 已由 FluentValidationFilter 自動處理

                // 設置默認分站權限
                if (parameters.StationIds == null || parameters.StationIds.Count == 0)
                {
                    parameters.StationIds = _authService.GetAvailableStationIds();
                }

                // 權限檢查
                var hasPermission = parameters.StationIds.All(id => _authService.HasStationPermission(id));
                if (!hasPermission)
                {
                    _logger.LogWarning("User {UserId} attempted to access unauthorized stations: {StationIds}",
                        _authService.GetCurrentUserId(), string.Join(",", parameters.StationIds));

                    return Forbid();
                }

                // 執行查詢
                var startTime = DateTime.Now;
                var result = await _parkingRecordService.GetRecordsListAsync(parameters);
                var duration = DateTime.Now - startTime;

                // 記錄性能
                await _performanceMonitor.LogQueryPerformanceAsync(
                    "ParkingRecords",
                    duration,
                    result.Data.Count,
                    new Dictionary<string, object>
                    {
                        ["TotalCount"] = result.TotalCount,
                        ["Page"] = result.Page,
                        ["Size"] = result.Size
                    });

                _logger.LogInformation("Successfully retrieved {Count} parking records for user {UserId}",
                    result.Data.Count, _authService.GetCurrentUserId());

                return Ok(Result<PagedResponse<ParkingRecordListResponse>>.SuccessResult(
                    result,
                    $"成功獲取 {result.Data.Count} 條記錄"));
            }
            catch (Exception ex)
            {
                await _performanceMonitor.LogErrorAsync("GetParkingRecords", ex,
                    new Dictionary<string, object>
                    {
                        ["UserId"] = _authService.GetCurrentUserId(),
                        ["Parameters"] = parameters
                    });

                _logger.LogError(ex, "Error occurred while getting parking records for user {UserId}",
                    _authService.GetCurrentUserId());

                return StatusCode(500, Result<PagedResponse<ParkingRecordListResponse>>.FailureResult(
                    "查詢記錄時發生內部錯誤", "INTERNAL_ERROR"));
            }
        }

        /// <summary>
        /// 獲取停車記錄總數
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="availableStationIds">可用分站ID</param>
        /// <returns>記錄總數</returns>
        [HttpGet("parking-records-count")]
        [ProducesResponseType(typeof(Result<int>), 200)]
        [ProducesResponseType(typeof(Result<int>), 500)]
        public async Task<ActionResult<Result<int>>> GetRecordsCount(
            [FromQuery] string keyword = "",
            [FromQuery] List<int>? availableStationIds = null)
        {
            using var operation = _performanceMonitor.StartOperation("GetParkingRecordsCount");

            try
            {
                if (availableStationIds == null || availableStationIds.Count == 0)
                {
                    availableStationIds = _authService.GetAvailableStationIds();
                }

                var count = await _parkingRecordService.GetRecordsCountAsync(keyword, availableStationIds);

                return Ok(Result<int>.SuccessResult(count, "成功獲取記錄總數"));
            }
            catch (Exception ex)
            {
                await _performanceMonitor.LogErrorAsync("GetParkingRecordsCount", ex);

                _logger.LogError(ex, "Error occurred while getting parking records count");

                return StatusCode(500, Result<int>.FailureResult(
                    "獲取記錄總數時發生內部錯誤", "INTERNAL_ERROR"));
            }
        }

        /// <summary>
        /// 導出停車記錄
        /// </summary>
        [HttpGet("parking-records-export")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 500)]
        public IActionResult ExportRecords(
            [FromQuery] string keyword = "",
            [FromQuery] List<int>? stationIds = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string format = "csv")
        {
            if (format.ToLower() != "csv")
            {
                return BadRequest(Result.FailureResult(
                    $"目前僅支持 CSV 格式的流式導出。",
                    "UNSUPPORTED_FORMAT"));
            }

            var fileName = $"parking-records_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

            return new PushStreamResult(async (stream, cancel) =>
            {
                try
                {
                    var writer = new StreamWriter(stream, new UTF8Encoding(true));
                    await using (writer)
                    {
                        await writer.WriteLineAsync(string.Join(",", "設備序號", "設備名稱", "分站名稱", "總車位", "已停車位", "剩餘車位", "使用率", "時間"));

                        var queryParams = new ParkingRecordQueryParameters
                        {
                            Keyword = keyword,
                            StationIds = stationIds ?? _authService.GetAvailableStationIds(),
                            StartDate = startDate,
                            EndDate = endDate,
                        };

                        int recordCount = 0;
                        await foreach (var record in _parkingRecordService.StreamAllParkingRecordsAsync(queryParams))
                        {
                            try
                            {
                                var line = string.Join(",",
                                    CsvUtils.EscapeCsvField(record.DeviceSerial),
                                    CsvUtils.EscapeCsvField(record.DeviceName),
                                    CsvUtils.EscapeCsvField(record.StationName),
                                    record.TotalSpaces,
                                    record.ParkedNum,
                                    record.AvailableSpaces,
                                    record.OccupancyRate.ToString(CultureInfo.InvariantCulture),
                                    CsvUtils.EscapeCsvField(record.Time)
                                );
                                await writer.WriteLineAsync(line);

                                recordCount++;
                                if (recordCount % 1000 == 0) // Flush every 1000 records
                                {
                                    await writer.FlushAsync();
                                    if (cancel.IsCancellationRequested)
                                    {
                                        _logger.LogWarning("Client disconnected during CSV export after {RecordCount} records.", recordCount);
                                        break; // Exit the loop
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error writing CSV line for record ID {RecordId}. Record data: {@Record}", record.Id, record);
                                throw; // Re-throw to stop the stream and indicate an error to the client.
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while exporting parking records");
                }
                finally
                {
                    await stream.FlushAsync();
                }
            }, "text/csv; charset=utf-8");
        }
    }
}