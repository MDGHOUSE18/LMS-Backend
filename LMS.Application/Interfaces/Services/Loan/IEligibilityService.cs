using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using EligibilityEvaluationResult = LMS.Application.Services.Loan.EligibilityResult;

namespace LMS.Application.Interfaces.Services.Loan
{
    public interface IEligibilityService
    {
        Task<EligibilityEvaluationResult> EvaluateAsync(
            Guid loanId,
            User user,
            LoanApplication loan,
            LoanFinancialDetails financialDetails);
    }
}
