using LMS.Application.DTOs.Dashboard;
using LMS.Application.Interfaces.Common;
using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LMS.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardController(
            ILoanService loanService,
            IPaymentScheduleService paymentScheduleService,
            ICurrentUserService currentUserService)
        {
            _loanService = loanService;
            _paymentScheduleService = paymentScheduleService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get customer dashboard data
        /// </summary>
        [HttpGet("customer")]
        [ProducesResponseType(typeof(CustomerDashboardResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                // Get all loans for the user
                var loans = await _loanService.GetLoansByUserIdAsync(userId);
                
                var dashboard = new CustomerDashboardResponse
                {
                    TotalApplicationsCount = loans.Count(),
                    ApprovedCount = loans.Count(l => l.Status == LoanStatusEnum.Approved || l.Status == LoanStatusEnum.Disbursed),
                    RejectedCount = loans.Count(l => l.Status == LoanStatusEnum.Rejected),
                    ActiveLoans = new List<LoanSummary>(),
                    NextEmiAmount = 0,
                    NextEmiDueDate = null
                };

                decimal totalOutstanding = 0;

                foreach (var loan in loans.Where(l => l.Status == LoanStatusEnum.Disbursed))
                {
                    var payments = await _paymentScheduleService.GetScheduleAsync(loan.Id);
                    var outstanding = payments
                        .Where(p => p.Status != PaymentStatus.Paid.ToString())
                        .Sum(p => p.PrincipalPart);
                    
                    totalOutstanding += outstanding;

                    var nextPayment = payments
                        .Where(p => p.Status == PaymentStatus.Pending.ToString())
                        .OrderBy(p => p.DueDate)
                        .FirstOrDefault();

                    dashboard.ActiveLoans.Add(new LoanSummary
                    {
                        LoanId = loan.Id,
                        ApplicationNumber = $"LMS{loan.Id.ToString().Substring(0, 8).ToUpper()}",
                        LoanAmount = loan.FinancialDetails?.LoanAmount ?? 0,
                        OutstandingBalance = outstanding,
                        MonthlyEMI = loan.CalculatedEMI ?? 0,
                        Status = loan.Status.ToString(),
                        AppliedDate = loan.CreatedAt,
                        NextDueDate = nextPayment?.DueDate
                    });

                    if (nextPayment != null && dashboard.NextEmiDueDate == null)
                    {
                        dashboard.NextEmiAmount = nextPayment.Amount;
                        dashboard.NextEmiDueDate = nextPayment.DueDate;
                    }
                }

                dashboard.TotalOutstandingAmount = totalOutstanding;

                // Add latest application
                var latestLoan = loans.OrderByDescending(l => l.CreatedAt).FirstOrDefault();
                if (latestLoan != null)
                {
                    dashboard.LatestApplication = new LoanSummary
                    {
                        LoanId = latestLoan.Id,
                        ApplicationNumber = $"LMS{latestLoan.Id.ToString().Substring(0, 8).ToUpper()}",
                        LoanAmount = latestLoan.FinancialDetails?.LoanAmount ?? 0,
                        OutstandingBalance = 0,
                        MonthlyEMI = latestLoan.CalculatedEMI ?? 0,
                        Status = latestLoan.Status.ToString(),
                        AppliedDate = latestLoan.CreatedAt
                    };
                }

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching dashboard data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get officer/admin dashboard data
        /// </summary>
        [HttpGet("officer")]
        [Authorize(Roles = "Officer,Admin")]
        [ProducesResponseType(typeof(OfficerDashboardResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOfficerDashboard()
        {
            try
            {
                var dashboard = new OfficerDashboardResponse
                {
                    PendingApplications = new List<PendingApplicationSummary>(),
                    Portfolio = new PortfolioSummary(),
                    SLA = new SlaMetrics
                    {
                        TargetSLADays = 3
                    }
                };

                // Get pending applications
                var pendingLoans = await _loanService.GetPendingApplicationsAsync();
                dashboard.PendingApplicationsCount = pendingLoans.Count();

                foreach (var loan in pendingLoans.OrderByDescending(l => l.CreatedAt))
                {
                    var daysPending = (int)(DateTime.UtcNow - loan.CreatedAt).TotalDays;
                    
                    dashboard.PendingApplications.Add(new PendingApplicationSummary
                    {
                        LoanId = loan.Id,
                        ApplicationNumber = $"LMS{loan.Id.ToString().Substring(0, 8).ToUpper()}",
                        CustomerName = "Customer Name", // Need to join with User entity
                        LoanAmount = loan.FinancialDetails?.LoanAmount ?? 0,
                        AppliedDate = loan.CreatedAt,
                        Status = loan.Status.ToString(),
                        DaysPending = daysPending,
                        HasEligibilityFlags = false
                    });
                }

                // Get portfolio metrics
                var allLoans = await _loanService.GetAllLoansAsync();
                var approvedLoans = allLoans.Where(l => l.Status == LoanStatusEnum.Approved || l.Status == LoanStatusEnum.Disbursed).ToList();
                var rejectedLoans = allLoans.Where(l => l.Status == LoanStatusEnum.Rejected).ToList();

                dashboard.Portfolio.TotalApprovedLoans = approvedLoans.Count;
                dashboard.Portfolio.TotalRejectedLoans = rejectedLoans.Count;
                dashboard.Portfolio.RejectionRate = allLoans.Any() 
                    ? (decimal)(rejectedLoans.Count * 100.0 / allLoans.Count) 
                    : 0;

                // Calculate average processing time
                var processedLoans = allLoans.Where(l => l.ApprovedDate.HasValue || l.RejectedDate.HasValue).ToList();
                if (processedLoans.Any())
                {
                    var avgDays = processedLoans.Average(l =>
                        ((l.ApprovedDate ?? l.RejectedDate ?? DateTime.UtcNow) - l.CreatedAt).TotalDays);
                    dashboard.Portfolio.AverageProcessingTimeDays = avgDays;
                }

                // SLA metrics
                var slaTargetDays = 3;
                var loansInPeriod = processedLoans.Where(l => l.CreatedAt > DateTime.UtcNow.AddDays(-30)).ToList();
                var withinSLA = loansInPeriod.Count(l => 
                    ((l.ApprovedDate ?? l.RejectedDate ?? DateTime.UtcNow) - l.CreatedAt).TotalDays <= slaTargetDays);

                dashboard.SLA.TotalProcessedInPeriod = loansInPeriod.Count;
                dashboard.SLA.ProcessedWithinSLA = withinSLA;
                dashboard.SLA.PercentageProcessedWithinSLA = loansInPeriod.Any()
                    ? (decimal)(withinSLA * 100.0 / loansInPeriod.Count)
                    : 100;

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching dashboard data", error = ex.Message });
            }
        }
    }
}
