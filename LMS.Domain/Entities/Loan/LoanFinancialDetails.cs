using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class LoanFinancialDetails
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }

        public decimal LoanAmount { get; set; }
        public int TenureMonths { get; set; }
        public decimal? InterestRate { get; set; }

        public decimal MonthlyIncome { get; set; }
        public decimal? ExistingEMI { get; set; }
    }
}
