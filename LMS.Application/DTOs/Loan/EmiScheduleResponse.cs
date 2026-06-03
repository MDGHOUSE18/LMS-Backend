using System;

namespace LMS.Application.DTOs.Loan
{
    public class EmiScheduleResponse
    {
        public int Month { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EMIAmount { get; set; }
        public decimal PrincipalComponent { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidDate { get; set; }
    }
}
