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
    public class DashboardController : ControllerBase
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;

        public DashboardController(
            ILoanRepository loanRepository,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository)
        {
            _loanRepository = loanRepository;
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userRepository.GetByIdAsync(userId);
                
                if (user == null)
                    return NotFound(new { message = "User not found" });

                var response = new DashboardDto();

                if (user.Role == UserRole.Customer)
                {
                    response.CustomerData = await GetCustomerDashboard(userId);
                }
                else if (user.Role == UserRole.Officer || user.Role == UserRole.Admin)
                {
                    response.OfficerData = await GetOfficerDashboard();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch dashboard", error = ex.Message });
            }
        }

        private async Task<CustomerDashboardData> GetCustomerDashboard(Guid userId)
        {
            var loans = await _loanRepository.GetByUserIdAsync(userId);
            var loanList = loans.ToList();

            var activeLoans = loanList.Where(l => l.Status == LoanStatus.Approved || l.Status == LoanStatus.Disbursed).ToList();
            var totalOutstanding = 0m;

            var loanSummaries = new List<LoanSummary>();
            foreach (var loan in loanList)
            {
                var outstanding = loan.Status == LoanStatus.Disbursed 
                    ? await _paymentRepository.GetOutstandingBalanceAsync(loan.LoanId)
                    : 0m;
                
                totalOutstanding += outstanding;

                var nextDueDate = DateTime.MaxValue;
                int remainingTenure = 0;
                
                if (loan.Status == LoanStatus.Disbursed && loan.Payments != null)
                {
                    var pendingPayments = loan.Payments.Where(p => p.Status == PaymentStatus.Pending).ToList();
                    if (pendingPayments.Any())
                    {
                        nextDueDate = pendingPayments.Min(p => p.DueDate);
                        remainingTenure = pendingPayments.Count;
                    }
                }

                loanSummaries.Add(new LoanSummary
                {
                    LoanId = loan.LoanId,
                    Amount = loan.Amount,
                    Status = loan.Status,
                    EmiAmount = loan.EmiAmount ?? 0,
                    NextDueDate = nextDueDate == DateTime.MaxValue ? DateTime.MinValue : nextDueDate,
                    OutstandingBalance = outstanding,
                    RemainingTenure = remainingTenure
                });
            }

            return new CustomerDashboardData
            {
                Loans = loanSummaries,
                TotalLoans = loanList.Count,
                ActiveLoans = activeLoans.Count,
                TotalOutstanding = totalOutstanding
            };
        }

        private async Task<OfficerDashboardData> GetOfficerDashboard()
        {
            var pendingLoans = (await _loanRepository.GetByStatusAsync(LoanStatus.UnderReview)).ToList();
            var submittedLoans = (await _loanRepository.GetByStatusAsync(LoanStatus.Submitted)).ToList();
            var allPending = pendingLoans.Concat(submittedLoans).ToList();

            var today = DateTime.UtcNow.Date;
            var approvedToday = (await _loanRepository.GetAllAsync())
                .Count(l => l.Status == LoanStatus.Approved && l.ApprovedDate?.Date == today);
            
            var rejectedToday = (await _loanRepository.GetAllAsync())
                .Count(l => l.Status == LoanStatus.Rejected && l.ApprovedDate?.Date == today);

            var totalPortfolio = (await _loanRepository.GetAllAsync())
                .Where(l => l.Status == LoanStatus.Disbursed)
                .Sum(l => l.Amount - (l.Payments?.Sum(p => p.PrincipalPart) ?? 0));

            // Calculate average processing time (mock calculation)
            var avgProcessingHours = allPending.Any() 
                ? allPending.Average(l => (DateTime.UtcNow - l.AppliedDate).TotalHours)
                : 0;

            // SLA adherence (loans processed within 3 days = 72 hours)
            var slaAdherence = allPending.Any()
                ? (double)allPending.Count(l => (DateTime.UtcNow - l.AppliedDate).TotalHours <= 72) / allPending.Count * 100
                : 100.0;

            var pendingQueue = allPending
                .Select(l => new LoanApplicationSummary
                {
                    LoanId = l.LoanId,
                    CustomerName = l.User?.Name ?? "Unknown",
                    Amount = l.Amount,
                    Status = l.Status,
                    AppliedDate = l.AppliedDate,
                    DaysInCurrentStatus = (int)(DateTime.UtcNow - l.AppliedDate).TotalDays
                })
                .OrderBy(l => l.AppliedDate)
                .Take(20)
                .ToList();

            return new OfficerDashboardData
            {
                PendingApplications = allPending.Count,
                ApprovedToday = approvedToday,
                RejectedToday = rejectedToday,
                TotalPortfolio = totalPortfolio,
                AverageProcessingTimeHours = Math.Round(avgProcessingHours, 2),
                SlaAdherencePercentage = Math.Round(slaAdherence, 2),
                PendingQueue = pendingQueue
            };
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("User not authenticated");
            return userId;
        }
    }
}
