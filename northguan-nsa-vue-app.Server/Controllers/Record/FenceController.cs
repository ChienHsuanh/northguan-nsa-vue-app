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
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace northguan_nsa_vue_app.Server.Controllers.Record
{
    /// <summary>
    /// 電子圍籬記錄控制器 - 重構版本
    /// 提供完整的錯誤處理、驗證和監控
    /// </summary>
    [ApiController]
    [Route("api")]
    [Authorize]
    public class FenceController(
                        IFenceRecordService fenceRecordService,
                        IAuthService authService,
                        IPerformanceMonitor performanceMonitor,
                        ILogger<FenceController> logger,
                        IWebHostEnvironment webHostEnvironment) : ControllerBase
    {
        private readonly IFenceRecordService _fenceRecordService = fenceRecordService;
        private readonly IAuthService _authService = authService;
        private readonly IPerformanceMonitor _performanceMonitor = performanceMonitor;
        private readonly ILogger<FenceController> _logger = logger;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;        /// <summary>
                                                                                              /// 獲取電子圍籬記錄列表
                                                                                              /// 支持高級過濾：事件類型、日期範圍、設備序號等
                                                                                              /// </summary>
                                                                                              /// <param name="parameters">查詢參數</param>
                                                                                              /// <returns>分頁的電子圍籬記錄列表</returns>
        [HttpGet("fence-records")]
        [ProducesResponseType(typeof(Result<PagedResponse<FenceRecordListResponse>>), 200)]
        [ProducesResponseType(typeof(Result<PagedResponse<FenceRecordListResponse>>), 400)]
        [ProducesResponseType(typeof(Result<PagedResponse<FenceRecordListResponse>>), 403)]
        [ProducesResponseType(typeof(Result<PagedResponse<FenceRecordListResponse>>), 500)]
        public async Task<ActionResult<Result<PagedResponse<FenceRecordListResponse>>>> GetRecordsList([FromQuery] FenceRecordQueryParameters parameters)
        {
            using var operation = _performanceMonitor.StartOperation("GetFenceRecords",
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

                // 記錄開始時間
                var startTime = DateTime.Now;

                var result = await _fenceRecordService.GetRecordsListAsync(parameters);
                var duration = DateTime.Now - startTime;

                // 記錄性能
                await _performanceMonitor.LogQueryPerformanceAsync(
                    "FenceRecords",
                    duration,
                    result.Data.Count,
                    new Dictionary<string, object>
                    {
                        ["TotalCount"] = result.TotalCount,
                        ["Page"] = result.Page,
                        ["Size"] = result.Size
                    });

                _logger.LogInformation("Successfully retrieved {Count} fence records for user {UserId}",
                    result.Data.Count, _authService.GetCurrentUserId());

                return Ok(Result<PagedResponse<FenceRecordListResponse>>.SuccessResult(
                    result,
                    $"成功獲取 {result.Data.Count} 條記錄"));
            }
            catch (Exception ex)
            {
                await _performanceMonitor.LogErrorAsync("GetFenceRecords", ex,
                    new Dictionary<string, object>
                    {
                        ["UserId"] = _authService.GetCurrentUserId(),
                        ["Parameters"] = parameters
                    });

                _logger.LogError(ex, "Error occurred while getting fence records for user {UserId}",
                    _authService.GetCurrentUserId());

                return StatusCode(500, Result<PagedResponse<FenceRecordListResponse>>.FailureResult(
                    "查詢記錄時發生內部錯誤", "INTERNAL_ERROR"));
            }
        }

        /// <summary>
        /// 獲取電子圍籬記錄總數
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="availableStationIds">可用分站ID</param>
        /// <returns>記錄總數</returns>
        [HttpGet("fence-records-count")]
        [ProducesResponseType(typeof(Result<int>), 200)]
        [ProducesResponseType(typeof(Result<int>), 500)]
        public async Task<ActionResult<Result<int>>> GetRecordsCount(
            [FromQuery] string keyword = "",
            [FromQuery] List<int>? availableStationIds = null)
        {
            using var operation = _performanceMonitor.StartOperation("GetFenceRecordsCount");

            try
            {
                if (availableStationIds == null || availableStationIds.Count == 0)
                {
                    availableStationIds = _authService.GetAvailableStationIds();
                }

                var count = await _fenceRecordService.GetRecordsCountAsync(keyword, availableStationIds);

                return Ok(Result<int>.SuccessResult(count, "成功獲取記錄總數"));
            }
            catch (Exception ex)
            {
                await _performanceMonitor.LogErrorAsync("GetFenceRecordsCount", ex);

                _logger.LogError(ex, "Error occurred while getting fence records count");

                return StatusCode(500, Result<int>.FailureResult(
                    "獲取記錄總數時發生內部錯誤", "INTERNAL_ERROR"));
            }
        }

        [HttpGet("fence-records-export")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 500)]
        public async Task<IActionResult> ExportRecords(
            [FromQuery] string keyword = "",
            [FromQuery] List<int>? stationIds = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string format = "xlsx")
        {
            var queryParams = new FenceRecordQueryParameters
            {
                Keyword = keyword,
                StationIds = stationIds ?? _authService.GetAvailableStationIds(),
                StartDate = startDate,
                EndDate = endDate,
            };

            if (format.ToLower() == "xlsx")
            {
                var fileName = $"fence-records_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                using (var workbook = new XSSFWorkbook())
                {
                    var sheet = workbook.CreateSheet("電子圍籬記錄");

                    // Headers
                    var headerRow = sheet.CreateRow(0);
                    headerRow.CreateCell(0).SetCellValue("分站名稱");
                    headerRow.CreateCell(1).SetCellValue("裝置名稱");
                    headerRow.CreateCell(2).SetCellValue("裝置序號");
                    headerRow.CreateCell(3).SetCellValue("事件");
                    headerRow.CreateCell(4).SetCellValue("時間");
                    headerRow.CreateCell(5).SetCellValue("圖片");

                    var creationHelper = workbook.GetCreationHelper();
                    var drawingPatriarch = sheet.CreateDrawingPatriarch();

                    int rowNum = 1;
                    await foreach (var record in _fenceRecordService.StreamAllFenceRecordsAsync(queryParams))
                    {
                        try
                        {
                            var dataRow = sheet.CreateRow(rowNum);
                            dataRow.HeightInPoints = 80;

                            dataRow.CreateCell(0).SetCellValue(record.StationName);
                            dataRow.CreateCell(1).SetCellValue(record.DeviceName);
                            dataRow.CreateCell(2).SetCellValue(record.DeviceSerial);
                            dataRow.CreateCell(3).SetCellValue(record.Event);
                            dataRow.CreateCell(4).SetCellValue(record.Time);

                            if (!string.IsNullOrEmpty(record.ImageUrl) && !record.ImageUrl.EndsWith("image-placeholder.png"))
                            {
                                try
                                {
                                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, record.ImageUrl.TrimStart('/'));
                                    imagePath = Path.GetFullPath(imagePath);

                                    if (System.IO.File.Exists(imagePath))
                                    {
                                        var imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                                        int pictureIdx = workbook.AddPicture(imageBytes, PictureType.PNG);

                                        var anchor = creationHelper.CreateClientAnchor();
                                        anchor.Col1 = 5;
                                        anchor.Row1 = rowNum;
                                        anchor.Col2 = 6;
                                        anchor.Row2 = rowNum + 1;

                                        drawingPatriarch.CreatePicture(anchor, pictureIdx);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Image file not found for record ID {RecordId} at path {ImagePath}", record.Id, imagePath);
                                        dataRow.CreateCell(5).SetCellValue("圖片文件不存在");
                                    }
                                }
                                catch (Exception imgEx)
                                {
                                    _logger.LogError(imgEx, "Error embedding image for record ID {RecordId} from path {ImageUrl}", record.Id, record.ImageUrl);
                                    dataRow.CreateCell(5).SetCellValue("圖片嵌入失敗");
                                }
                            }
                            else
                            {
                                dataRow.CreateCell(5).SetCellValue("無圖片");
                            }
                            rowNum++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing record ID {RecordId} for XLSX export. Record data: {@Record}", record.Id, record);
                        }
                    }

                    for (int i = 0; i < 5; i++) sheet.AutoSizeColumn(i);
                    sheet.SetColumnWidth(5, 25 * 256);

                    using (var memoryStream = new MemoryStream())
                    {
                        workbook.Write(memoryStream, true);
                        return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            else if (format.ToLower() == "csv")
            {
                var fileName = $"fence-records_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                return new PushStreamResult(async (stream, cancel) =>
                {
                    try
                    {
                        var writer = new StreamWriter(stream, new UTF8Encoding(true));
                        await using (writer)
                        {
                            await writer.WriteLineAsync(string.Join(",", "設備序號", "設備名稱", "分站名稱", "事件", "圖片網址", "時間"));

                            int recordCount = 0;
                            await foreach (var record in _fenceRecordService.StreamAllFenceRecordsAsync(queryParams))
                            {
                                if (cancel.IsCancellationRequested)
                                {
                                    _logger.LogWarning("Client disconnected during CSV export.");
                                    break;
                                }

                                try
                                {
                                    var line = string.Join(",",
                                        CsvUtils.EscapeCsvField(record.DeviceSerial),
                                        CsvUtils.EscapeCsvField(record.DeviceName),
                                        CsvUtils.EscapeCsvField(record.StationName),
                                        CsvUtils.EscapeCsvField(record.Event),
                                        CsvUtils.EscapeCsvField(record.ImageUrl),
                                        CsvUtils.EscapeCsvField(record.Time)
                                    );
                                    await writer.WriteLineAsync(line);

                                    recordCount++;
                                    if (recordCount % 1000 == 0)
                                    {
                                        await writer.FlushAsync();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error writing CSV line for record ID {RecordId}. Record data: {@Record}", record.Id, record);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException)
                        {
                            _logger.LogWarning("Client disconnected during CSV export.");
                        }
                        else
                        {
                            _logger.LogError(ex, "Error occurred while exporting fence records to CSV");
                        }
                    }
                }, "text/csv; charset=utf-8");
            }
            else
            {
                return BadRequest(Result.FailureResult(
                    $"不支持的匯出格式 '{format}'。請使用 'csv' 或 'xlsx'。",
                    "UNSUPPORTED_FORMAT"));
            }
        }
    }
}