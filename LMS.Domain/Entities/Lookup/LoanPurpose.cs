using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Lookup
{
    public class LoanPurpose
    {
        public int Id { get; set; }
        public string PurposeName { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    }
}
