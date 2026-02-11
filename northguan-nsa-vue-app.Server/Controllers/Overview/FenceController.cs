using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;
using System.ComponentModel.DataAnnotations;

namespace northguan_nsa_vue_app.Server.Controllers.Overview
{
    [ApiController]
    [Route("api/overview/fence")]
    [Authorize]
    public class FenceController : ControllerBase
    {
        private readonly IFenceOverviewService _fenceOverviewService;
        private readonly IAuthService _authService;
        private readonly ILogger<FenceController> _logger;

        public FenceController(
            IFenceOverviewService fenceOverviewService, 
            IAuthService authService,
            ILogger<FenceController> logger)
        {
            _fenceOverviewService = fenceOverviewService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取圍欄記錄歷史
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <returns>圍欄記錄歷史數據</returns>
        [HttpGet("recent-record-history")]
        [ProducesResponseType(typeof(FenceRecordHistoryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<FenceRecordHistoryResponse>> GetRecentRecordHistory(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800)
        {
            try
            {
                _logger.LogInformation("Getting fence record history for station {StationId} with time range {TimeRange} seconds", stationID, timeRange);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new FenceRecordHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new FenceRecordHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var history = await _fenceOverviewService.GetRecentRecordHistoryAsync(stationID, timeRangeSeconds, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} fence record history records", history.Data?.Count ?? 0);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fence record history for station {StationId}", stationID);
                return StatusCode(500, new FenceRecordHistoryResponse 
                { 
                    Success = false, 
                    Message = "獲取圍欄記錄歷史時發生錯誤" 
                });
            }
        }

        /// <summary>
        /// 獲取圍籬最近記錄詳細資訊
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <param name="limit">返回記錄數量限制</param>
        /// <returns>圍籬最近記錄詳細資訊</returns>
        [HttpGet("recent-record")]
        [ProducesResponseType(typeof(FenceRecentRecordDetailResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<FenceRecentRecordDetailResponse>> GetRecentRecord(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800,
            [FromQuery] [Range(1, 100)] int limit = 5)
        {
            try
            {
                _logger.LogInformation("Getting fence recent records for station {StationId} with time range {TimeRange} seconds and limit {Limit}", 
                    stationID, timeRange, limit);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new FenceRecentRecordDetailResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new FenceRecentRecordDetailResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var records = await _fenceOverviewService.GetRecentRecordAsync(stationID, timeRangeSeconds, limit, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} fence recent records", records.Data?.Count ?? 0);
                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fence recent records for station {StationId}", stationID);
                return StatusCode(500, new FenceRecentRecordDetailResponse 
                { 
                    Success = false, 
                    Message = "獲取圍欄最近記錄時發生錯誤" 
                });
            }
        }

        /// <summary>
        /// 獲取圍欄最新記錄
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <returns>圍欄最新記錄數據</returns>
        [HttpGet("latest-record")]
        [ProducesResponseType(typeof(FenceLatestRecordResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<FenceLatestRecordResponse>> GetLatestRecord([FromQuery] int stationID = 0)
        {
            try
            {
                _logger.LogInformation("Getting fence latest record for station {StationId}", stationID);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new FenceLatestRecordResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new FenceLatestRecordResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                var record = await _fenceOverviewService.GetLatestRecordAsync(stationID, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} fence latest records", record.Data?.Count ?? 0);
                return Ok(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fence latest record for station {StationId}", stationID);
                return StatusCode(500, new FenceLatestRecordResponse 
                { 
                    Success = false, 
                    Message = "獲取圍欄最新記錄時發生錯誤" 
                });
            }
        }
    }
}