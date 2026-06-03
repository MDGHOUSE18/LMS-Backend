using LMS.Application.Interfaces.Services.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public class EligibilityService : IEligibilityService
    {
        public Task EvaluateAsync(int loanId)
        {
            throw new NotImplementedException();
        }
    }
}
