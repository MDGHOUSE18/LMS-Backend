using System;

namespace LoanManagementSystem.Core.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public UserRole Role { get; set; }
        public bool IsVerified { get; set; }
    }

    public class LoanApplicationDto
    {
        public Guid LoanId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public int TenureMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public decimal? EmiAmount { get; set; }
        public LoanStatus Status { get; set; }
        public string? Purpose { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal ExistingEmis { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? DisbursedDate { get; set; }
        public string? RejectionReason { get; set; }
        public EligibilityResult? EligibilityStatus { get; set; }
    }

    public class EligibilityResult
    {
        public bool IsEligible { get; set; }
        public string Status { get; set; } = string.Empty; // Eligible, Refer, Rejected
        public List<string> Flags { get; set; } = new();
        public string? Message { get; set; }
    }

    public class DocumentUploadDto
    {
        public Guid LoanId { get; set; }
        public DocumentType Type { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class DocumentResponseDto
    {
        public Guid DocId { get; set; }
        public Guid LoanId { get; set; }
        public DocumentType Type { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? VerifiedBy { get; set; }
    }

    public class ApproveLoanDto
    {
        public Guid OfficerId { get; set; }
        public string? Comments { get; set; }
    }

    public class RejectLoanDto
    {
        public Guid OfficerId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class EmiScheduleDto
    {
        public decimal MonthlyEmi { get; set; }
        public decimal TotalAmountPayable { get; set; }
        public decimal TotalInterest { get; set; }
        public List<AmortizationItem> Schedule { get; set; } = new();
    }

    public class AmortizationItem
    {
        public int Month { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public PaymentStatus Status { get; set; }
        public int EmiMonth { get; set; }
        public decimal PrincipalPart { get; set; }
        public decimal InterestPart { get; set; }
    }

    public class DashboardDto
    {
        public CustomerDashboardData? CustomerData { get; set; }
        public OfficerDashboardData? OfficerData { get; set; }
    }

    public class CustomerDashboardData
    {
        public List<LoanSummary> Loans { get; set; } = new();
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    public class OfficerDashboardData
    {
        public int PendingApplications { get; set; }
        public int ApprovedToday { get; set; }
        public int RejectedToday { get; set; }
        public decimal TotalPortfolio { get; set; }
        public double AverageProcessingTimeHours { get; set; }
        public double SlaAdherencePercentage { get; set; }
        public List<LoanApplicationSummary> PendingQueue { get; set; } = new();
    }

    public class LoanSummary
    {
        public Guid LoanId { get; set; }
        public decimal Amount { get; set; }
        public LoanStatus Status { get; set; }
        public decimal EmiAmount { get; set; }
        public DateTime NextDueDate { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int RemainingTenure { get; set; }
    }

    public class LoanApplicationSummary
    {
        public Guid LoanId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public LoanStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }
        public int DaysInCurrentStatus { get; set; }
    }

    public class AuditLogDto
    {
        public Guid LogId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}
