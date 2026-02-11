using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class MapController : ControllerBase
    {
        private readonly IMapService _mapService;

        public MapController(IMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpGet("landmarks")]
        public async Task<ActionResult<LandmarksResponse>> GetLandmarks()
        {
            var landmarks = await _mapService.GetLandmarksAsync();
            return Ok(landmarks);
        }

        [HttpGet("landmarks/{id}/parking")]
        public async Task<ActionResult<ParkingLandmarkResponse>> GetLandmarkParking(int id)
        {
            var parking = await _mapService.GetLandmarkParkingAsync(id);
            return Ok(parking);
        }

        [HttpGet("landmarks/{id}/traffic")]
        public async Task<ActionResult<TrafficLandmarkResponse>> GetLandmarkTraffic(int id)
        {
            var traffic = await _mapService.GetLandmarkTrafficAsync(id);
            return Ok(traffic);
        }

        [HttpGet("landmarks/{id}/crowd")]
        public async Task<ActionResult<CrowdLandmarkResponse>> GetLandmarkCrowd(int id)
        {
            var crowd = await _mapService.GetLandmarkCrowdAsync(id);
            return Ok(crowd);
        }

        [HttpGet("landmarks/{id}/fence")]
        public async Task<ActionResult<FenceLandmarkResponse>> GetLandmarkFence(int id)
        {
            var fence = await _mapService.GetLandmarkFenceAsync(id);
            return Ok(fence);
        }

        [HttpGet("landmarks/{id}/highResolution")]
        public async Task<ActionResult<HighResolutionLandmarkResponse>> GetLandmarkHighResolution(int id)
        {
            var highResolution = await _mapService.GetLandmarkHighResolutionAsync(id);
            return Ok(highResolution);
        }
    }
}