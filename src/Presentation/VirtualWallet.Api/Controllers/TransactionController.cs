using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Services;

namespace VirtualWallet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [Authorize]
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
        {
            await _transactionService.MakeTransferAsync(request);

            return Ok(new { message = "Transfer completed successfully." });
        }

        [Authorize]
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
        {
            await _transactionService.DepositAsync(request);
            return Ok(new { message = "Deposit completed successfully." });
        }

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var history = await _transactionService.GetHistoryAsync(pageNumber, pageSize);
            return Ok(history);
        }
    }
}
