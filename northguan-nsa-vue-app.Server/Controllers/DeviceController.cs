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
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly IAuthService _authService;

        public DeviceController(IDeviceService deviceService, IAuthService authService)
        {
            _deviceService = deviceService;
            _authService = authService;
        }

        [HttpGet("devices")]
        public async Task<ActionResult<List<DeviceListResponse>>> GetDevices([FromQuery] DeviceListRequest request)
        {
            var availableStationIds = _authService.IsAdmin()
                ? null
                : _authService.GetAvailableStationIds();

            // 如果 Size 設為 -1，則返回所有設備（不分頁）
            var devices = await _deviceService.GetDevicesAsync(
                request.Type,
                request.Keyword,
                request.Size == -1 ? null : request.Page,
                request.Size == -1 ? null : request.Size,
                availableStationIds);

            return Ok(devices);
        }

        [HttpGet("device-count")]
        public async Task<ActionResult<CountResponse>> GetDeviceCount([FromQuery] string type, [FromQuery] string keyword = "")
        {
            var availableStationIds = _authService.IsAdmin()
                ? null
                : _authService.GetAvailableStationIds();

            var count = await _deviceService.GetDeviceCountAsync(type, keyword, availableStationIds);
            return Ok(new CountResponse { Count = count });
        }

        [HttpPost("devices")]
        public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceRequest request)
        {
            await _deviceService.CreateDeviceAsync(request);
            return NoContent();
        }

        [HttpPut("devices/{id}")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] UpdateDeviceRequest request)
        {
            await _deviceService.UpdateDeviceAsync(id, request);
            return NoContent();
        }

        [HttpDelete("devices/{id}")]
        public async Task<IActionResult> DeleteDevice(int id, [FromQuery] string type)
        {
            await _deviceService.DeleteDeviceAsync(id, type);
            return NoContent();
        }

        [HttpGet("devices-status")]
        public async Task<ActionResult<List<DeviceStatusResponse>>> GetDevicesStatus([FromQuery] int page = 1, [FromQuery] int size = 100, [FromQuery] string keyword = "")
        {
            var availableStationIds = _authService.GetAvailableStationIds();
            
            // 如果 size 設為 -1，則返回所有設備狀態（不分頁）
            var devices = await _deviceService.GetDevicesStatusAsync(
                size == -1 ? null : page, 
                size == -1 ? null : size, 
                keyword, 
                availableStationIds);
            return Ok(devices);
        }

        [HttpGet("devices-status-count")]
        public async Task<ActionResult<CountResponse>> GetDevicesStatusCount()
        {
            var availableStationIds = _authService.GetAvailableStationIds();
            var count = await _deviceService.GetDevicesStatusCountAsync(availableStationIds);
            return Ok(new CountResponse { Count = count });
        }

        [HttpGet("devices-status-logs")]
        public async Task<ActionResult<List<DeviceStatusLogResponse>>> GetDeviceStatusLogs([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string keyword = "")
        {
            var availableStationIds = _authService.GetAvailableStationIds();
            var logs = await _deviceService.GetDeviceStatusLogsAsync(page, size, keyword, availableStationIds);
            return Ok(logs);
        }

        [HttpGet("devices-status-logs-count")]
        public async Task<ActionResult<CountResponse>> GetDeviceStatusLogCount([FromQuery] string keyword = "")
        {
            var availableStationIds = _authService.GetAvailableStationIds();
            var count = await _deviceService.GetDeviceStatusLogCountAsync(keyword, availableStationIds);
            return Ok(new CountResponse { Count = count });
        }

        [HttpGet("devices/{deviceId}/stream")]
        [AllowAnonymous] // Allow access without auth for stream viewer
        public async Task<IActionResult> GetDeviceStream(
            int deviceId, 
            [FromQuery] string type = "crowd", 
            [FromQuery] string channelid = "cam1")
        {
            try
            {
                // Get device information securely
                var device = await _deviceService.GetDeviceByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found");
                }

                // Validate device type matches expected type
                if (!string.Equals(device.Type, type, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Device type mismatch");
                }

                // Check if device has API URL for streaming
                if (string.IsNullOrEmpty(device.ApiUrl) || !device.ApiUrl.Contains("GetAIJpeg"))
                {
                    return BadRequest("Device does not support streaming");
                }

                // Create HTTP client to proxy the image request
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                // Build the actual API URL
                var baseUrl = GetBaseUrl(device.ApiUrl);
                var imageUrl = $"{baseUrl}/GetAIJpeg?channelid={channelid}&i={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

                // Make request to device API
                var response = await httpClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(503, "Device stream unavailable");
                }

                // Return the image stream
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                
                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                // Log error but don't expose internal details
                Console.WriteLine($"Device stream error: {ex.Message}");
                return StatusCode(500, "Stream error");
            }
        }

        private static string GetBaseUrl(string apiUrl)
        {
            try
            {
                var uri = new Uri(apiUrl);
                return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            }
            catch
            {
                return apiUrl.TrimEnd('/');
            }
        }
    }
}