using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories.Loan
{
    public interface ILoanRepository : IRepository<LoanApplication>
    {
        Task<List<LoanApplication>> GetByUserIdAsync(Guid userId);
        Task<List<LoanApplication>> GetPendingApplicationsAsync();
        Task<List<LoanApplication>> GetAllLoansAsync();
        Task<bool> ExistsRecentApplicationAsync(Guid userId, DateTime sinceDate);
    }
}
