using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs.Loan
{
    public class CreateLoanRequest
    {
        public int PurposeId { get; set; }
        public int EmploymentTypeId { get; set; }

        public decimal LoanAmount { get; set; }
        public int TenureMonths { get; set; }

        public decimal MonthlyIncome { get; set; }
        public decimal? ExistingEMI { get; set; }
    }
}
