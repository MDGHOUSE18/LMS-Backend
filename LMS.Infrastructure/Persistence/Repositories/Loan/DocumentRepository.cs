using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Loan
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Document> GetByIdAsync(Guid id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _context.Documents.ToListAsync();
        }

        public void Add(Document entity)
        {
            _context.Documents.Add(entity);
        }

        public void Update(Document entity)
        {
            _context.Documents.Update(entity);
        }

        public void Delete(Document entity)
        {
            _context.Documents.Remove(entity);
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<List<Document>> GetByLoanIdAsync(Guid loanId)
        {
            return await _context.Documents
                .Where(d => d.LoanId == loanId)
                .OrderBy(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetPendingVerificationAsync()
        {
            return await _context.Documents
                .Where(d => d.VerificationStatus == VerificationStatus.Pending)
                .OrderBy(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }
    }
}
