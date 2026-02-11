using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Models;
using northguan_nsa_vue_app.Server.Services;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class StationController : ControllerBase
    {
        private readonly IStationService _stationService;
        private readonly IAuthService _authService;
        private readonly IDeviceService _deviceService;

        public StationController(IStationService stationService, IAuthService authService, IDeviceService deviceService)
        {
            _stationService = stationService;
            _authService = authService;
            _deviceService = deviceService;
        }

        [HttpGet("stations")]
        public async Task<ActionResult<List<StationResponse>>> GetStations([FromQuery] StationListRequest request)
        {
            var availableStationIds = _authService.IsAdmin()
                ? null
                : _authService.GetAvailableStationIds();

            var stations = await _stationService.GetStationsAsync(
                request.Page,
                request.Size,
                request.Keyword,
                availableStationIds);

            var stationIds = stations.Select(s => s.Id).ToList();

            // Get devices for all stations
            var crowdDevices = await _deviceService.GetCrowdDevicesByStationIdsAsync(stationIds);
            var parkingDevices = await _deviceService.GetParkingDevicesByStationIdsAsync(stationIds);
            var trafficDevices = await _deviceService.GetTrafficDevicesByStationIdsAsync(stationIds);
            var fenceDevices = await _deviceService.GetFenceDevicesByStationIdsAsync(stationIds);
            var highResolutionDevices = await _deviceService.GetHighResolutionDevicesByStationIdsAsync(stationIds);

            var response = stations.Select(station => new StationResponse
            {
                Id = station.Id,
                Name = station.Name,
                Lat = station.Lat,
                Lng = station.Lng,
                LineToken = station.LineToken,
                EnableNotify = station.EnableNotify,
                CvpLocations = station.CvpLocations ?? "",
                CrowdDevices = crowdDevices.Where(d => d.StationId == station.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        StationID = d.StationId,
                        Name = d.Name,
                        Serial = d.Serial,
                        Lat = d.Lat,
                        Lng = d.Lng,
                        Area = d.Area,
                        VideoUrl = d.VideoUrl,
                        ApiUrl = d.ApiUrl
                    }).ToList(),
                ParkingDevices = parkingDevices.Where(d => d.StationId == station.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        StationID = d.StationId,
                        Name = d.Name,
                        Serial = d.Serial,
                        Lat = d.Lat,
                        Lng = d.Lng,
                        ApiUrl = d.ApiUrl,
                        NumberOfParking = d.NumberOfParking
                    }).ToList(),
                TrafficDevices = trafficDevices.Where(d => d.StationId == station.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        StationID = d.StationId,
                        Name = d.Name,
                        Serial = d.Serial,
                        City = d.City,
                        ETag_number = d.ETagNumber,
                        Lat = d.Lat,
                        Lng = d.Lng,
                        SpeedLimit = d.SpeedLimit
                    }).ToList(),
                FenceDevices = fenceDevices.Where(d => d.StationId == station.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        StationID = d.StationId,
                        Name = d.Name,
                        Serial = d.Serial,
                        Lat = d.Lat,
                        Lng = d.Lng
                    }).ToList(),
                HighResolutionDevices = highResolutionDevices.Where(d => d.StationId == station.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        StationID = d.StationId,
                        Name = d.Name,
                        Serial = d.Serial,
                        Lat = d.Lat,
                        Lng = d.Lng,
                        VideoUrl = d.VideoUrl
                    }).ToList()
            }).ToList();

            return Ok(response);
        }

        [HttpGet("station-count")]
        public async Task<ActionResult<CountResponse>> GetStationCount([FromQuery] string keyword = "")
        {
            var availableStationIds = _authService.IsAdmin()
                ? null
                : _authService.GetAvailableStationIds();

            var count = await _stationService.GetStationCountAsync(keyword, availableStationIds);
            return Ok(new CountResponse { Count = count });
        }

        [HttpGet("stations/{id}")]
        public async Task<ActionResult<StationResponse>> GetStationDetail(int id)
        {
            // Check station permission
            if (!_authService.HasStationPermission(id))
            {
                return Forbid();
            }

            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            return Ok(new StationResponse
            {
                Id = station.Id,
                Name = station.Name,
                Lat = station.Lat,
                Lng = station.Lng,
                LineToken = station.LineToken ?? string.Empty,
                EnableNotify = station.EnableNotify,
                CvpLocations = station.CvpLocations ?? string.Empty
            });
        }

        [HttpPost("stations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStation([FromBody] CreateStationRequest request)
        {
            var station = new Station
            {
                Name = request.Name,
                Lat = request.Lat,
                Lng = request.Lng,
                LineToken = request.LineToken,
                EnableNotify = request.EnableNotify,
                CvpLocations = request.CvpLocations
            };

            await _stationService.CreateStationAsync(station);
            return NoContent();
        }

        [HttpPut("stations/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStation(int id, [FromBody] UpdateStationRequest request)
        {
            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            station.Name = request.Name;
            station.Lat = request.Lat;
            station.Lng = request.Lng;
            station.LineToken = request.LineToken;
            station.EnableNotify = request.EnableNotify;
            station.CvpLocations = request.CvpLocations;

            await _stationService.UpdateStationAsync(station);
            return NoContent();
        }

        [HttpDelete("stations/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStation(int id)
        {
            await _stationService.DeleteStationAsync(id);
            return NoContent();
        }
    }
}