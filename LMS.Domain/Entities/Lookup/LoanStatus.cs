using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Lookup
{
    public class LoanStatus
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
        public ICollection<LoanStatusHistory> FromStatusHistories { get; set; } = new List<LoanStatusHistory>();
        public ICollection<LoanStatusHistory> ToStatusHistories { get; set; } = new List<LoanStatusHistory>();
    }
}
