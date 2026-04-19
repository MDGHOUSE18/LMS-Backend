using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs.Loan
{
    public class UpdateLoanRequest : CreateLoanRequest
    {
        public int LoanId { get; set; }
    }
}
