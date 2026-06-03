using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        
        public decimal Amount { get; set; } // EMI amount
        public int EmiMonth { get; set; } // 1-60
        
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        
        public string Status { get; set; } // Pending, Paid, Missed
        
        public decimal PrincipalPart { get; set; }
        public decimal InterestPart { get; set; }
        
        public decimal? PenaltyAmount { get; set; }
        public DateTime? LastReminderSent { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual LoanApplication? Loan { get; set; }
    }
}
