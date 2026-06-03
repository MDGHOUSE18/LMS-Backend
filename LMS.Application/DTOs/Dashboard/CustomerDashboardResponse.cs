using System;

namespace LMS.Application.DTOs.Dashboard
{
    public class CustomerDashboardResponse
    {
        public List<LoanSummary> ActiveLoans { get; set; } = new();
        public LoanSummary? LatestApplication { get; set; }
        public int TotalApplicationsCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public decimal NextEmiAmount { get; set; }
        public DateTime? NextEmiDueDate { get; set; }
    }

    public class LoanSummary
    {
        public Guid LoanId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal MonthlyEMI { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public DateTime? NextDueDate { get; set; }
    }
}
