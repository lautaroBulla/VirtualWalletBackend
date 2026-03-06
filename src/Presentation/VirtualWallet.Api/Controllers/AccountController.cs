using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [Authorize]
        [HttpGet("myAccount")]
        public async Task<IActionResult> GetMyAccount()
        {
            var accountInfo = await _accountService.GetMyAccountAsync();
            return Ok(accountInfo);
        }

        [Authorize]
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
        {
            await _accountService.DepositAsync(request);
            return Ok(new { message = "Deposit completed successfully." });
        }
    }
}
