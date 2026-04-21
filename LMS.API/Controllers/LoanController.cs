using LMS.Application.DTOs.Loan;
using LMS.Application.Interfaces.Services.Loan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/loan")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
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
        public async Task<IActionResult> Submit(int loanId)
        {
            await _loanService.SubmitLoanAsync(loanId);
            return Ok();
        }
    }
}
