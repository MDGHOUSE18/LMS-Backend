using LMS.Application.DTOs.Loan;
using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Services.Loan
{
    public interface ILoanService
    {
        Task<int> CreateDraftAsync(CreateLoanRequest request);
        Task UpdateDraftAsync(UpdateLoanRequest request);
        Task SubmitLoanAsync(int loanId);
        Task<LoanApplication?> GetByIdAsync(Guid loanId);
        Task<List<LoanApplication>> GetLoansByUserIdAsync(Guid userId);
        Task<List<LoanApplication>> GetPendingApplicationsAsync();
        Task<List<LoanApplication>> GetAllLoansAsync();
        Task ApproveLoanAsync(Guid loanId, Guid approvedBy);
        Task RejectLoanAsync(Guid loanId, string reason, Guid rejectedBy);
        Task GenerateEmiScheduleAsync(Guid loanId);
    }
}
