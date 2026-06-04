using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories.Loan
{
    public interface IDocumentRepository : IRepository<Document>
    {
        void Add(Document entity);
        void Delete(Document entity);
        Task<List<Document>> GetByLoanIdAsync(Guid loanId);
        Task<List<Document>> GetByUserIdAsync(Guid userId);
        Task<List<Document>> GetPendingVerificationAsync();
    }
}
