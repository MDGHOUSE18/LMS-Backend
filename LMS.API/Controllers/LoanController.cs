using LMS.Application.DTOs.Loan;
using LMS.Application.Interfaces.Common;
using LMS.Application.Interfaces.Services.Loan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/loan")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ICurrentUserService _currentUserService;

        public LoanController(ILoanService loanService, ICurrentUserService currentUserService)
        {
            _loanService = loanService;
            _currentUserService = currentUserService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateLoanRequest request)
        {
            var id = await _loanService.CreateDraftAsync(request);
            return Ok(id);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateLoanRequest request)
        {
            await _loanService.UpdateDraftAsync(request);
            return Ok();
        }

        [HttpPost("submit/{loanId}")]
        public async Task<IActionResult> Submit(Guid loanId)
        {
            await _loanService.SubmitLoanAsync(loanId);
            return Ok();
        }

        [HttpPut("approve/{loanId}")]
        [Authorize(Roles = "Officer,Admin")]
        public async Task<IActionResult> Approve(Guid loanId)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            await _loanService.ApproveLoanAsync(loanId, userId);
            return Ok(new { message = "Loan approved successfully" });
        }

        [HttpPut("reject/{loanId}")]
        [Authorize(Roles = "Officer,Admin")]
        public async Task<IActionResult> Reject(Guid loanId, [FromBody] RejectLoanRequest request)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { message = "Rejection reason is required" });

            await _loanService.RejectLoanAsync(loanId, request.Reason, userId);
            return Ok(new { message = "Loan rejected successfully" });
        }
    }

    public class RejectLoanRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
