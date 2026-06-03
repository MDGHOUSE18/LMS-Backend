using System;

namespace LMS.Application.DTOs.Dashboard
{
    public class OfficerDashboardResponse
    {
        public int PendingApplicationsCount { get; set; }
        public List<PendingApplicationSummary> PendingApplications { get; set; } = new();
        public PortfolioSummary Portfolio { get; set; } = new();
        public SlaMetrics SLA { get; set; } = new();
    }

    public class PendingApplicationSummary
    {
        public Guid LoanId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DaysPending { get; set; }
        public bool HasEligibilityFlags { get; set; }
    }

    public class PortfolioSummary
    {
        public decimal TotalOutstandingAmount { get; set; }
        public int TotalApprovedLoans { get; set; }
        public int TotalRejectedLoans { get; set; }
        public decimal RejectionRate { get; set; }
        public double AverageProcessingTimeDays { get; set; }
        public int NPACount { get; set; }
    }

    public class SlaMetrics
    {
        public decimal PercentageProcessedWithinSLA { get; set; }
        public int TargetSLADays { get; set; }
        public int TotalProcessedInPeriod { get; set; }
        public int ProcessedWithinSLA { get; set; }
    }
}
