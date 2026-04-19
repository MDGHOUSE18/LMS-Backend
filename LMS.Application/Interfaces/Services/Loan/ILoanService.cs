using LMS.Application.DTOs.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Services.Loan
{
    public interface ILoanService
    {
        Task<int> CreateDraftAsync(CreateLoanRequest request);
        Task UpdateDraftAsync(UpdateLoanRequest request);
        Task SubmitLoanAsync(int loanId);
    }
}
