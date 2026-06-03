using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Workflow
{
    public class LoanStatusHistory
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }
        public int? FromStatusId { get; set; }
        public int ToStatusId { get; set; }
        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Comments { get; set; }

        // Navigation Properties
        public LoanApplication LoanApplication { get; set; } = null!;
        public LoanStatus? FromStatus { get; set; }
        public LoanStatus ToStatus { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
