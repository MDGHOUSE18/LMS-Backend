using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories.Loan
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<List<Document>> GetByLoanIdAsync(Guid loanId);
        Task<Document?> GetByIdAsync(Guid id);
        Task<List<Document>> GetPendingVerificationAsync();
        Task<List<Document>> GetByUserIdAsync(Guid userId);
    }
}
