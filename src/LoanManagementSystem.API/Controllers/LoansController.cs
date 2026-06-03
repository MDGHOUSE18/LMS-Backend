using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Core.DTOs;
using LoanManagementSystem.Core.Entities;
using LoanManagementSystem.Core.Interfaces;
using LoanManagementSystem.Core.Services;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly EligibilityService _eligibilityService;
        private readonly WorkflowService _workflowService;
        private readonly EmiCalculatorService _emiCalculator;

        public LoansController(
            ILoanRepository loanRepository,
            IDocumentRepository documentRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            EligibilityService eligibilityService,
            WorkflowService workflowService,
            EmiCalculatorService emiCalculator)
        {
            _loanRepository = loanRepository;
            _documentRepository = documentRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _eligibilityService = eligibilityService;
            _workflowService = workflowService;
            _emiCalculator = emiCalculator;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] LoanApplicationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Check for duplicate application
                if (await _loanRepository.HasActiveLoanAsync(userId))
                    return Conflict(new { message = "An application for this amount is already pending. Please wait or contact support." });

                // Validate loan amount
                if (dto.Amount < 50000 || dto.Amount > 5000000)
                    return BadRequest(new { message = "Loan amount must be between ₹50,000 and ₹50,00,000" });

                // Validate tenure
                if (dto.TenureMonths < 6 || dto.TenureMonths > 60 || dto.TenureMonths % 3 != 0)
                    return BadRequest(new { message = "Tenure must be 6-60 months in multiples of 3" });

                // Validate interest rate
                if (dto.AnnualInterestRate < 6 || dto.AnnualInterestRate > 18)
                    return BadRequest(new { message = "Interest rate must be between 6% and 18%" });

                // Get user for age validation
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // Create loan application
                var loan = new LoanApplication
                {
                    LoanId = Guid.NewGuid(),
                    UserId = userId,
                    Amount = dto.Amount,
                    TenureMonths = dto.TenureMonths,
                    AnnualInterestRate = dto.AnnualInterestRate,
                    Status = LoanStatus.Draft,
                    Purpose = dto.Purpose,
                    MonthlyIncome = dto.MonthlyIncome,
                    ExistingEmis = dto.ExistingEmis,
                    EmploymentType = dto.EmploymentType,
                    AppliedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _loanRepository.AddAsync(loan);

                // Calculate EMI
                var emiResult = _emiCalculator.CalculateEmi(loan.Amount, loan.AnnualInterestRate, loan.TenureMonths);
                loan.EmiAmount = emiResult.MonthlyEmi;
                await _loanRepository.UpdateAsync(loan);

                // Log audit
                await CreateAuditLog(userId, "Create", "Loan", loan.LoanId, $"Loan application created for ₹{loan.Amount}");

                var response = new LoanApplicationDto
                {
                    LoanId = loan.LoanId,
                    UserId = loan.UserId,
                    Amount = loan.Amount,
                    TenureMonths = loan.TenureMonths,
                    AnnualInterestRate = loan.AnnualInterestRate,
                    EmiAmount = loan.EmiAmount,
                    Status = loan.Status,
                    Purpose = loan.Purpose,
                    MonthlyIncome = loan.MonthlyIncome,
                    ExistingEmis = loan.ExistingEmis,
                    EmploymentType = loan.EmploymentType,
                    AppliedDate = loan.AppliedDate
                };

                return CreatedAtAction(nameof(GetLoanById), new { id = loan.LoanId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Application submission failed", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userRepository.GetByIdAsync(userId);
                
                var loan = await _loanRepository.GetWithDetailsAsync(id);
                if (loan == null)
                    return NotFound(new { message = "Loan not found" });

                // Authorization check
                if (user.Role != UserRole.Admin && user.Role != UserRole.Officer && loan.UserId != userId)
                    return Forbid();

                var documents = await _documentRepository.GetByLoanIdAsync(id);
                var emiSchedule = _emiCalculator.GenerateAmortizationSchedule(loan.Amount, loan.AnnualInterestRate, loan.TenureMonths);

                var response = new
                {
                    loan = new LoanApplicationDto
                    {
                        LoanId = loan.LoanId,
                        UserId = loan.UserId,
                        Amount = loan.Amount,
                        TenureMonths = loan.TenureMonths,
                        AnnualInterestRate = loan.AnnualInterestRate,
                        EmiAmount = loan.EmiAmount,
                        Status = loan.Status,
                        Purpose = loan.Purpose,
                        MonthlyIncome = loan.MonthlyIncome,
                        ExistingEmis = loan.ExistingEmis,
                        EmploymentType = loan.EmploymentType,
                        AppliedDate = loan.AppliedDate,
                        ApprovedDate = loan.ApprovedDate,
                        DisbursedDate = loan.DisbursedDate,
                        RejectionReason = loan.RejectionReason
                    },
                    documents = documents.Select(d => new DocumentResponseDto
                    {
                        DocId = d.DocId,
                        LoanId = d.LoanId,
                        Type = d.Type,
                        VerificationStatus = d.VerificationStatus,
                        UploadedAt = d.UploadedAt,
                        VerifiedAt = d.VerifiedAt,
                        RejectionReason = d.RejectionReason
                    }),
                    emiSchedule = new EmiScheduleDto
                    {
                        MonthlyEmi = emiSchedule.MonthlyEmi,
                        TotalAmountPayable = emiSchedule.TotalAmountPayable,
                        TotalInterest = emiSchedule.TotalInterest,
                        Schedule = emiSchedule.Schedule.Select(s => new AmortizationItem
                        {
                            Month = s.Month,
                            PrincipalAmount = s.PrincipalAmount,
                            InterestAmount = s.InterestAmount,
                            OutstandingBalance = s.OutstandingBalance
                        }).ToList()
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch loan details", error = ex.Message });
            }
        }

        [HttpPut("{id}/submit")]
        public async Task<IActionResult> SubmitLoan(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var loan = await _loanRepository.GetWithDetailsAsync(id);
                
                if (loan == null || loan.UserId != userId)
                    return NotFound(new { message = "Loan not found" });

                if (loan.Status != LoanStatus.Draft)
                    return BadRequest(new { message = "Only draft applications can be submitted" });

                // Run eligibility check
                var user = await _userRepository.GetByIdAsync(userId);
                var eligibilityResult = _eligibilityService.ValidateEligibility(loan, user);

                // Transition workflow
                await _workflowService.TransitionToSubmitted(loan, eligibilityResult);

                await CreateAuditLog(userId, "Submit", "Loan", loan.LoanId, $"Loan submitted for review. Status: {eligibilityResult.Status}");

                return Ok(new { 
                    message = "Loan application submitted successfully",
                    status = loan.Status,
                    eligibilityStatus = eligibilityResult.Status,
                    flags = eligibilityResult.Flags
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Submission failed", error = ex.Message });
            }
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Officer,Admin")]
        public async Task<IActionResult> ApproveLoan(Guid id, [FromBody] ApproveLoanDto dto)
        {
            try
            {
                var officerId = GetCurrentUserId();
                var loan = await _loanRepository.GetWithDetailsAsync(id);
                
                if (loan == null)
                    return NotFound(new { message = "Loan not found" });

                if (loan.Status != LoanStatus.UnderReview)
                    return BadRequest(new { message = "Only loans under review can be approved" });

                // Check all documents are verified
                var documents = await _documentRepository.GetByLoanIdAsync(id);
                if (!documents.All(d => d.VerificationStatus == VerificationStatus.Verified))
                    return BadRequest(new { message = "All KYC documents must be verified before approval" });

                await _workflowService.TransitionToApproved(loan, officerId, dto.Comments);
                await CreateAuditLog(officerId, "Approve", "Loan", loan.LoanId, $"Loan approved by officer. Comments: {dto.Comments}");

                return Ok(new { message = "Loan approved successfully", status = loan.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Approval failed", error = ex.Message });
            }
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Officer,Admin")]
        public async Task<IActionResult> RejectLoan(Guid id, [FromBody] RejectLoanDto dto)
        {
            try
            {
                var officerId = GetCurrentUserId();
                var loan = await _loanRepository.GetWithDetailsAsync(id);
                
                if (loan == null)
                    return NotFound(new { message = "Loan not found" });

                if (string.IsNullOrEmpty(dto.Reason))
                    return BadRequest(new { message = "Rejection reason is required" });

                await _workflowService.TransitionToRejected(loan, officerId, dto.Reason);
                await CreateAuditLog(officerId, "Reject", "Loan", loan.LoanId, $"Loan rejected. Reason: {dto.Reason}");

                return Ok(new { message = "Loan rejected", status = loan.Status, reason = dto.Reason });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Rejection failed", error = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("User not authenticated");
            return userId;
        }

        private async Task CreateAuditLog(Guid? userId, string action, string entityType, Guid entityId, string details)
        {
            var auditLog = new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Details = details,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            await _auditLogRepository.AddAsync(auditLog);
        }
    }
}
