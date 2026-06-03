using System;

namespace LMS.Application.DTOs.Loan
{
    public class ApproveLoanRequest
    {
        public Guid LoanId { get; set; }
    }

    public class RejectLoanRequest
    {
        public Guid LoanId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
