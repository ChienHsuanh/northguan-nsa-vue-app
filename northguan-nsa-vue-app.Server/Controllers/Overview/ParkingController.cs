using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;
using System.ComponentModel.DataAnnotations;

namespace northguan_nsa_vue_app.Server.Controllers.Overview
{
    [ApiController]
    [Route("api/overview/parking")]
    [Authorize]
    public class ParkingController : ControllerBase
    {
        private readonly IParkingOverviewService _parkingOverviewService;
        private readonly IAuthService _authService;
        private readonly ILogger<ParkingController> _logger;

        public ParkingController(
            IParkingOverviewService parkingOverviewService, 
            IAuthService authService,
            ILogger<ParkingController> logger)
        {
            _parkingOverviewService = parkingOverviewService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取停車場轉換歷史記錄
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <returns>停車場轉換歷史數據</returns>
        [HttpGet("recent-conversion-history")]
        [ProducesResponseType(typeof(ParkingConversionHistoryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ParkingConversionHistoryResponse>> GetRecentConversionHistory(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800)
        {
            try
            {
                _logger.LogInformation("Getting parking conversion history for station {StationId} with time range {TimeRange} seconds", stationID, timeRange);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new ParkingConversionHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new ParkingConversionHistoryResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var history = await _parkingOverviewService.GetRecentConversionHistoryAsync(stationID, timeRangeSeconds, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} parking conversion history records", history.Data?.Count ?? 0);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parking conversion history for station {StationId}", stationID);
                return StatusCode(500, new ParkingConversionHistoryResponse 
                { 
                    Success = false, 
                    Message = "獲取停車場轉換歷史時發生錯誤" 
                });
            }
        }

        /// <summary>
        /// 獲取停車場使用率統計
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <param name="timeRange">時間範圍（秒），默認7天</param>
        /// <param name="limit">返回記錄數量限制</param>
        /// <returns>停車場使用率統計數據</returns>
        [HttpGet("recent-parking-rate")]
        [ProducesResponseType(typeof(ParkingRateResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ParkingRateResponse>> GetRecentParkingRate(
            [FromQuery] int stationID = 0, 
            [FromQuery] int timeRange = 604800,
            [FromQuery] [Range(1, 100)] int limit = 5)
        {
            try
            {
                _logger.LogInformation("Getting parking rates for station {StationId} with time range {TimeRange} seconds and limit {Limit}", 
                    stationID, timeRange, limit);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new ParkingRateResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new ParkingRateResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                // 限制時間範圍在合理範圍內（最小60秒，最大30天）
                var timeRangeSeconds = Math.Min(Math.Max(timeRange, 60), 2592000);
                var rates = await _parkingOverviewService.GetRecentParkingRateAsync(stationID, timeRangeSeconds, limit, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} parking rate records", rates.Data?.Count ?? 0);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parking rates for station {StationId}", stationID);
                return StatusCode(500, new ParkingRateResponse 
                { 
                    Success = false, 
                    Message = "獲取停車場使用率時發生錯誤" 
                });
            }
        }
    }
}