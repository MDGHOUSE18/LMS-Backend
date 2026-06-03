using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class EligibilityResult
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }
        public bool IsEligible { get; set; }
        public int? Age { get; set; }
        public decimal? MonthlyIncome { get; set; }
        public decimal? DTI { get; set; }
        public decimal? LoanToIncomeRatio { get; set; }
        public string? Remarks { get; set; }
        public DateTime EvaluatedAt { get; set; }

        // Navigation Properties
        public LoanApplication LoanApplication { get; set; } = null!;
    }
}
