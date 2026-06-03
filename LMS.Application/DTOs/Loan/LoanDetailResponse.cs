using System;

namespace LMS.Application.DTOs.Loan
{
    public class LoanDetailResponse
    {
        public Guid LoanId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public int TenureMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public decimal MonthlyEMI { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? DisbursedDate { get; set; }
        public string? RejectionReason { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public decimal ExistingEMI { get; set; }
        public bool IsEligible { get; set; }
        public List<string> EligibilityFlags { get; set; } = new();
    }
}
