using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LMS.Infrastructure.Persistence.Repositories.Loan
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly LMSDbContext _context;

        public DocumentRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<Document?> GetByIdAsync(Guid id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await _context.Documents.ToListAsync();
        }

        public async Task<IEnumerable<Document>> FindAsync(
            Expression<Func<Document, bool>> predicate)
        {
            return await _context.Documents
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Document> AddAsync(Document entity)
        {
            await _context.Documents.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public void Add(Document entity)
        {
            _context.Documents.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Document entity)
        {
            _context.Documents.Update(entity);
            _context.SaveChanges();
        }

        public void Remove(Document entity)
        {
            _context.Documents.Remove(entity);
            _context.SaveChanges();
        }

        public void Delete(Document entity)
        {
            _context.Documents.Remove(entity);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<Document> entities)
        {
            _context.Documents.RemoveRange(entities);
            _context.SaveChanges();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Documents
                .AnyAsync(x => x.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Documents.CountAsync();
        }

        public async Task<int> CountAsync(
            Expression<Func<Document, bool>> predicate)
        {
            return await _context.Documents.CountAsync(predicate);
        }

        public async Task<List<Document>> GetByLoanIdAsync(Guid loanId)
        {
            return await _context.Documents
                .Where(x => x.LoanId == loanId)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Documents
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Document>> GetPendingVerificationAsync()
        {
            return await _context.Documents
                .Where(x => x.VerificationStatus == 1)
                .ToListAsync();
        }
    }
}