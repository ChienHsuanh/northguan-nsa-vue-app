using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Controllers.Overview
{
    [ApiController]
    [Route("api/overview")]
    [Authorize]
    public class HighResolutionController : ControllerBase
    {
        private readonly IHighResolutionOverviewService _highResolutionOverviewService;
        private readonly IAuthService _authService;
        private readonly ILogger<HighResolutionController> _logger;

        public HighResolutionController(
            IHighResolutionOverviewService highResolutionOverviewService, 
            IAuthService authService,
            ILogger<HighResolutionController> logger)
        {
            _highResolutionOverviewService = highResolutionOverviewService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取高解析度設備概覽信息
        /// </summary>
        /// <param name="stationID">站點ID，0表示所有可訪問的站點</param>
        /// <returns>高解析度設備概覽數據</returns>
        [HttpGet("highResolution")]
        [ProducesResponseType(typeof(HighResolutionOverviewResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<HighResolutionOverviewResponse>> GetOverviewInfo([FromQuery] int stationID = 0)
        {
            try
            {
                _logger.LogInformation("Getting high resolution overview info for station {StationId}", stationID);
                
                var availableStationIds = _authService.GetAvailableStationIds();
                
                if (!availableStationIds.Any())
                {
                    _logger.LogWarning("User has no accessible stations");
                    return Ok(new HighResolutionOverviewResponse 
                    { 
                        Success = false, 
                        Message = "無可訪問的站點" 
                    });
                }

                if (stationID > 0 && !availableStationIds.Contains(stationID))
                {
                    _logger.LogWarning("User attempted to access unauthorized station {StationId}", stationID);
                    return BadRequest(new HighResolutionOverviewResponse 
                    { 
                        Success = false, 
                        Message = "無權限訪問指定站點" 
                    });
                }

                var info = await _highResolutionOverviewService.GetOverviewInfoAsync(stationID, availableStationIds);
                
                _logger.LogInformation("Successfully retrieved {Count} high resolution device records", info.Data?.Count ?? 0);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high resolution overview info for station {StationId}", stationID);
                return StatusCode(500, new HighResolutionOverviewResponse 
                { 
                    Success = false, 
                    Message = "獲取高解析度設備概覽時發生錯誤" 
                });
            }
        }
    }
}