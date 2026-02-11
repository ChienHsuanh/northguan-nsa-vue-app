using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;
using System.ComponentModel.DataAnnotations;

namespace northguan_nsa_vue_app.Server.Controllers.Overview
{
    [ApiController]
    [Route("api/overview/traffic")]
    [Authorize]
    public class TrafficController : ControllerBase
    {
        private readonly ITrafficOverviewService _trafficOverviewService;
        private readonly IAuthService _authService;
        private readonly ILogger<TrafficController> _logger;

        public TrafficController(
            ITrafficOverviewService trafficOverviewService, 
            IAuthService authService,
            ILogger<TrafficController> logger)
        {
            _trafficOverviewService = trafficOverviewService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取交通路況歷史記錄
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <returns>交通路況歷史數據</returns>
        [HttpGet("recent-road-condition-history")]
        [ProducesResponseType(typeof(TrafficRoadConditionHistoryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TrafficRoadConditionHistoryResponse>> GetRecentRoadConditionHistory(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800)
        {
            try
            {
                _logger.LogInformation("Getting traffic road condition history for station {StationId} with time range {TimeRange} seconds", stationID, timeRange);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new TrafficRoadConditionHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new TrafficRoadConditionHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var history = await _trafficOverviewService.GetRecentRoadConditionHistoryAsync(stationID, timeRangeSeconds, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} traffic road condition history records", history.Data?.Count ?? 0);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting traffic road condition history for station {StationId}", stationID);
                return StatusCode(500, new TrafficRoadConditionHistoryResponse 
                { 
                    Success = false, 
                    Message = "獲取交通路況歷史時發生錯誤" 
                });
            }
        }

        /// <summary>
        /// 獲取交通路況統計
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <param name="limit">返回記錄數量限制</param>
        /// <returns>交通路況統計數據</returns>
        [HttpGet("recent-road-condition")]
        [ProducesResponseType(typeof(TrafficRoadConditionResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TrafficRoadConditionResponse>> GetRecentRoadCondition(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800, 
            [FromQuery] [Range(1, 100)] int limit = 5)
        {
            try
            {
                _logger.LogInformation("Getting traffic road conditions for station {StationId} with time range {TimeRange} seconds and limit {Limit}", 
                    stationID, timeRange, limit);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new TrafficRoadConditionResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new TrafficRoadConditionResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var conditions = await _trafficOverviewService.GetRecentRoadConditionAsync(stationID, timeRangeSeconds, limit, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} traffic road condition records", conditions.Data?.Count ?? 0);
                return Ok(conditions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting traffic road conditions for station {StationId}", stationID);
                return StatusCode(500, new TrafficRoadConditionResponse 
                { 
                    Success = false, 
                    Message = "獲取交通路況統計時發生錯誤" 
                });
            }
        }
    }
}