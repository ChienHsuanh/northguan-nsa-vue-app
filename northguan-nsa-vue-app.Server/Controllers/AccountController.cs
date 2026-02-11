using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using northguan_nsa_vue_app.Server.DTOs;
using northguan_nsa_vue_app.Server.Services;

namespace northguan_nsa_vue_app.Server.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Roles = "Admin")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<List<UserResponse>>> GetAccountList([FromQuery] int page = 1, [FromQuery] int size = 20, [FromQuery] string keyword = "")
        {
            var accounts = await _accountService.GetAccountListAsync(page, size, keyword);
            return Ok(accounts);
        }

        [HttpGet("account-count")]
        public async Task<ActionResult<CountResponse>> GetAccountCount([FromQuery] string keyword = "")
        {
            var count = await _accountService.GetAccountCountAsync(keyword);
            return Ok(new CountResponse { Count = count });
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            await _accountService.CreateAccountAsync(request);
            return NoContent();
        }

        [HttpPut("accounts/{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] UpdateAccountRequest request)
        {
            await _accountService.UpdateAccountAsync(id, request);
            return NoContent();
        }

        [HttpDelete("accounts/{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            await _accountService.DeleteAccountAsync(id);
            return NoContent();
        }
    }
}