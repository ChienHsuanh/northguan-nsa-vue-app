using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.Services;
using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class SystemSettingController : ControllerBase
    {
        private readonly ISystemSettingService _systemSettingService;

        public SystemSettingController(ISystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        [HttpGet("systemSetting")]
        public async Task<ActionResult<SystemSettingResponse>> GetSystemSetting()
        {
            var settings = await _systemSettingService.GetSystemSettingAsync();
            return Ok(settings);
        }

        [HttpPut("systemSetting")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSystemSetting([FromBody] UpdateSystemSettingRequest request)
        {
            await _systemSettingService.UpdateSystemSettingAsync(request);
            return NoContent();
        }
    }
}
