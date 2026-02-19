using Microsoft.AspNetCore.Mvc;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api")]
    public class ThirdPartyApiController : ControllerBase
    {
        private readonly IThirdPartyService _thirdPartyService;

        public ThirdPartyApiController(IThirdPartyService thirdPartyService)
        {
            _thirdPartyService = thirdPartyService;
        }

        [HttpPost("geofence/fence")]
        public async Task<ActionResult<CreateFenceRecordResponse>> CreateFenceRecord([FromBody] CreateFenceRecordRequest request)
        {
            var response = await _thirdPartyService.CreateFenceRecordAsync(request);
            return Ok(response);
        }

        [HttpPost("geofence/heartbeat")]
        public async Task<ActionResult<UpdateFenceHeartbeatResponse>> UpdateFenceHeartbeat([FromBody] UpdateFenceHeartbeatRequest request)
        {
            var response = await _thirdPartyService.UpdateFenceHeartbeatAsync(request);
            return Ok(response);
        }

        [HttpPost("pedestrian/count")]
        public async Task<ActionResult<CreateCrowdRecordResponse>> CreateCrowdRecord([FromBody] CreateCrowdRecordRequest request)
        {
            var response = await _thirdPartyService.CreateCrowdRecordAsync(request);
            return Ok(response);
        }

        [HttpPost("parking/altobParking")]
        public async Task<ActionResult<CreateParkingRecordResponse>> CreateApParkingRecord([FromBody] CreateParkingRecordRequest request)
        {
            var response = await _thirdPartyService.CreateParkingRecordAsync(request);
            return Ok(response);
        }

        [HttpGet("station/list")]
        public async Task<ActionResult<List<ThirdPartyStationInfo>>> GetStationList()
        {
            var stations = await _thirdPartyService.GetStationListAsync();
            return Ok(stations);
        }

        [HttpGet("pedestrian/list")]
        public async Task<ActionResult<List<ThirdPartyFenceDeviceInfo>>> GetFenceDeviceList()
        {
            var devices = await _thirdPartyService.GetFenceDeviceListAsync();
            return Ok(devices);
        }

        [HttpGet("pedestrian/op_list")]
        public async Task<ActionResult<List<ThirdPartyCrowdDeviceInfo>>> GetCrowdDeviceList()
        {
            var devices = await _thirdPartyService.GetCrowdDeviceListAsync();
            return Ok(devices);
        }

        [HttpGet("parking/op_list")]
        public async Task<ActionResult<List<ThirdPartyParkingDeviceInfo>>> GetParkingDeviceList()
        {
            var devices = await _thirdPartyService.GetParkingDeviceListAsync();
            return Ok(devices);
        }

        [HttpGet("etag/op_list")]
        public async Task<ActionResult<List<ThirdPartyTrafficDeviceInfo>>> GetTrafficDeviceList()
        {
            var devices = await _thirdPartyService.GetTrafficDeviceListAsync();
            return Ok(devices);
        }

        [HttpGet("zero_touch/make_excel")]
        public async Task<ActionResult<FileStreamResult>> ZeroTouchFileMaker()
        {
            var fileBytes = await _thirdPartyService.CreateZeroTouchExcelAsync();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "zero-touch.xlsx");
        }

        /// <summary>
        /// RtspToHttp 拉取所有圍籬設備+區域設定
        /// </summary>
        [HttpGet("geofence/config")]
        public async Task<ActionResult<List<FenceDeviceConfigDto>>> GetFenceDeviceConfigs()
        {
            var configs = await _thirdPartyService.GetFenceDeviceConfigsAsync();
            return Ok(configs);
        }

        /// <summary>
        /// RtspToHttp 拉取所有設備設定（統一端點）
        /// </summary>
        [HttpGet("devices/all-config")]
        public async Task<ActionResult<List<UnifiedDeviceConfigDto>>> GetAllDeviceConfigs()
        {
            var configs = await _thirdPartyService.GetAllDeviceConfigsAsync();
            return Ok(configs);
        }

        /// <summary>
        /// RtspToHttp viewer 存圍籬多邊形到 DB
        /// </summary>
        [HttpPost("geofence/zones")]
        public async Task<ActionResult<UpdateFenceZonesResponse>> UpdateFenceZones([FromBody] UpdateFenceZonesRequest request)
        {
            try
            {
                var response = await _thirdPartyService.UpdateFenceZonesAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}