using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Common;
using FluentValidation;
using northguan_nsa_vue_app.Server.Services.Monitoring;
using System.Text;
using System.IO;
using System.Globalization;
using northguan_nsa_vue_app.Server.Utils;

namespace northguan_nsa_vue_app.Server.Controllers.Record
{
    /// <summary>
    /// 人流記錄控制器 - 重構版本
    /// 提供完整的錯誤處理、驗證和監控
    /// </summary>
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CrowdController(
        ICrowdRecordService crowdRecordService,
        IAuthService authService,
        IPerformanceMonitor performanceMonitor,
        ILogger<CrowdController> logger) : ControllerBase
    {
        private readonly ICrowdRecordService _crowdRecordService = crowdRecordService;
        private readonly IAuthService _authService = authService;
        private readonly IPerformanceMonitor _performanceMonitor = performanceMonitor;
        private readonly ILogger<CrowdController> _logger = logger;

        /// <summary>
        /// 獲取人流記錄列表
        /// </summary>
        [HttpGet("crowd-records")]
        [ProducesResponseType(typeof(Result<PagedResponse<CrowdRecordListResponse>>), 200)]
        [ProducesResponseType(typeof(Result<PagedResponse<CrowdRecordListResponse>>), 400)]
        [ProducesResponseType(typeof(Result<PagedResponse<CrowdRecordListResponse>>), 500)]
        public async Task<ActionResult<Result<PagedResponse<CrowdRecordListResponse>>>> GetRecordsList(
            [FromQuery] CrowdRecordQueryParameters parameters)
        {
            using var operation = _performanceMonitor.StartOperation("GetCrowdRecords",
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
                var result = await _crowdRecordService.GetRecordsListAsync(parameters);
                var duration = DateTime.Now - startTime;

                // 記錄性能
                await _performanceMonitor.LogQueryPerformanceAsync(
                    "CrowdRecords",
                    duration,
                    result.Data.Count,
                    new Dictionary<string, object>
                    {
                        ["TotalCount"] = result.TotalCount,
                        ["Page"] = result.Page,
                        ["Size"] = result.Size
                    });

                _logger.LogInformation("Successfully retrieved {Count} crowd records for user {UserId}",
                    result.Data.Count, _authService.GetCurrentUserId());

                return Ok(Result<PagedResponse<CrowdRecordListResponse>>.SuccessResult(
                    result,
                    $"成功獲取 {result.Data.Count} 條記錄"));
            }
            catch (Exception ex)
            {
                await _performanceMonitor.LogErrorAsync("GetCrowdRecords", ex,
                    new Dictionary<string, object>
                    {
                        ["UserId"] = _authService.GetCurrentUserId(),
                        ["Parameters"] = parameters
                    });

                _logger.LogError(ex, "Error occurred while getting crowd records for user {UserId}",
                    _authService.GetCurrentUserId());

                return StatusCode(500, Result<PagedResponse<CrowdRecordListResponse>>.FailureResult(
                    "查詢記錄時發生內部錯誤", "INTERNAL_ERROR"));
            }
        }

        /// <summary>
        /// 獲取人流記錄總數
        /// </summary>
        [HttpGet("crowd-records-count")]
        [ProducesResponseType(typeof(Result<int>), 200)]
        [ProducesResponseType(typeof(Result<int>), 500)]
        public async Task<ActionResult<Result<int>>> GetRecordsCount(
            [FromQuery] string keyword = "",
            [FromQuery] List<int>? availableStationIds = null)
        {
            using var operation = _performanceMonitor.StartOperation("GetCrowdRecordsCount");

            try
            {
                if (availableStationIds == null || availableStationIds.Count == 0)
                {
                    availableStationIds = _authService.GetAvailableStationIds();
                }

                var count = await _crowdRecordService.GetRecordsCountAsync(keyword, availableStationIds);

                return Ok(Result<int>.SuccessResult(count, "成功獲取記錄總數"));
            }
            catch (Exception ex)
            {
                await _performanceMonitor.LogErrorAsync("GetCrowdRecordsCount", ex);

                _logger.LogError(ex, "Error occurred while getting crowd records count");

                return StatusCode(500, Result<int>.FailureResult(
                    "獲取記錄總數時發生內部錯誤", "INTERNAL_ERROR"));
            }
        }

        /// <summary>
        /// 導出人流記錄
        /// </summary>
        [HttpGet("crowd-records-export")]
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

            var fileName = $"crowd-records_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

            return new PushStreamResult(async (stream, cancel) =>
            {
                try
                {
                    var writer = new StreamWriter(stream, new UTF8Encoding(true)); // Use BOM for Excel compatibility
                    await using (writer)
                    {
                        await writer.WriteLineAsync(string.Join(",", "設備序號", "設備名稱", "分站名稱", "區域", "人數", "密度", "時間"));

                        var queryParams = new CrowdRecordQueryParameters
                        {
                            Keyword = keyword,
                            StationIds = stationIds ?? _authService.GetAvailableStationIds(),
                            StartDate = startDate,
                            EndDate = endDate,
                        };

                        int recordCount = 0;
                        await foreach (var record in _crowdRecordService.StreamAllCrowdRecordsAsync(queryParams))
                        {
                            try
                            {
                                var line = string.Join(",",
                                    CsvUtils.EscapeCsvField(record.DeviceSerial),
                                    CsvUtils.EscapeCsvField(record.DeviceName),
                                    CsvUtils.EscapeCsvField(record.StationName),
                                    record.Area,
                                    record.PeopleCount,
                                    CrowdDensityToStatus(record.Density),
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
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while exporting crowd records");
                    // Cannot write to response here as it might be already sent
                }
                finally
                {
                    await stream.FlushAsync();
                }
            }, "text/csv; charset=utf-8");
        }

        private static string CrowdDensityToStatus(double density)
        {
            if (density < 0.7) return "擁擠";
            if (density < 1.2) return "稍擠";
            return "空曠";
        }
    }
}