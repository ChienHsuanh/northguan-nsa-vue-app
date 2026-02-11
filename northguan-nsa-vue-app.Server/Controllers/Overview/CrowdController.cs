using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;
using System.ComponentModel.DataAnnotations;

namespace northguan_nsa_vue_app.Server.Controllers.Overview
{
    [ApiController]
    [Route("api/overview/crowd")]
    [Authorize]
    public class CrowdController : ControllerBase
    {
        private readonly ICrowdOverviewService _crowdOverviewService;
        private readonly IAuthService _authService;
        private readonly ILogger<CrowdController> _logger;

        public CrowdController(
            ICrowdOverviewService crowdOverviewService, 
            IAuthService authService,
            ILogger<CrowdController> logger)
        {
            _crowdOverviewService = crowdOverviewService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取人群容量歷史記錄
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <returns>人群容量歷史數據</returns>
        [HttpGet("recent-capacity-history")]
        [ProducesResponseType(typeof(CrowdCapacityHistoryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CrowdCapacityHistoryResponse>> GetRecentCapacityHistory(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800)
        {
            try
            {
                _logger.LogInformation("Getting crowd capacity history for station {StationId} with time range {TimeRange} seconds", stationID, timeRange);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new CrowdCapacityHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new CrowdCapacityHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var history = await _crowdOverviewService.GetRecentCapacityHistoryAsync(stationID, timeRangeSeconds, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} crowd capacity history records", history.Data?.Count ?? 0);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting crowd capacity history for station {StationId}", stationID);
                return StatusCode(500, new CrowdCapacityHistoryResponse 
                { 
                    Success = false, 
                    Message = "獲取人群容量歷史時發生錯誤" 
                });
            }
        }

        /// <summary>
        /// 獲取人群密度統計
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <param name="limit">返回記錄數量限制</param>
        /// <returns>人群密度統計數據</returns>
        [HttpGet("recent-capacity-rate")]
        [ProducesResponseType(typeof(CrowdCapacityRateResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CrowdCapacityRateResponse>> GetRecentCapacityRate(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800, 
            [FromQuery] [Range(1, 100)] int limit = 5)
        {
            try
            {
                _logger.LogInformation("Getting crowd capacity rates for station {StationId} with time range {TimeRange} seconds and limit {Limit}", 
                    stationID, timeRange, limit);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new CrowdCapacityRateResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new CrowdCapacityRateResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var rates = await _crowdOverviewService.GetRecentCapacityRateAsync(stationID, timeRangeSeconds, limit, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} crowd capacity rate records", rates.Data?.Count ?? 0);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting crowd capacity rates for station {StationId}", stationID);
                return StatusCode(500, new CrowdCapacityRateResponse 
                { 
                    Success = false, 
                    Message = "獲取人群密度統計時發生錯誤" 
                });
            }
        }
    }
}