using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class LoanFinancialDetails
    {
        public Guid Id { get; set; }
        public Guid LoanApplicationId { get; set; }

        public decimal LoanAmount { get; set; }
        public int TenureMonths { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal Emi { get; set; }

        public decimal MonthlyIncome { get; set; }
        public decimal? ExistingEMI { get; set; }
        // Navigation Property (1:1 Relationship)
        public LoanApplication LoanApplication { get; set; } = null!;
    }
}
