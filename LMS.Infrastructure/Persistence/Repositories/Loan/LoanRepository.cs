using LMS.Application.Interfaces.Repositories;
using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Enums;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Loan
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LMSDbContext _dbContext;
        
        public LoanRepository(LMSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LoanApplication?> GetByIdAsync(Guid id)
        {
            return await _dbContext.LoanApplications
                .Include(l => l.FinancialDetails)
                .Include(l => l.Documents)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<LoanApplication?> GetByIdAsync(int id)
        {
            return await _dbContext.LoanApplications
                .Include(l => l.FinancialDetails)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<LoanApplication>> GetAllAsync()
        {
            return await _dbContext.LoanApplications
                .Include(l => l.FinancialDetails)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoanApplication>> FindAsync(System.Linq.Expressions.Expression<Func<LoanApplication, bool>> predicate)
        {
            return await _dbContext.LoanApplications
                .Include(l => l.FinancialDetails)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<LoanApplication> AddAsync(LoanApplication entity)
        {
            await _dbContext.LoanApplications.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public void Update(LoanApplication entity)
        {
            _dbContext.LoanApplications.Update(entity);
        }

        public void Remove(LoanApplication entity)
        {
            _dbContext.LoanApplications.Remove(entity);
        }

        public void RemoveRange(IEnumerable<LoanApplication> entities)
        {
            _dbContext.LoanApplications.RemoveRange(entities);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.LoanApplications.AnyAsync(l => l.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _dbContext.LoanApplications.CountAsync();
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<LoanApplication, bool>> predicate)
        {
            return await _dbContext.LoanApplications.CountAsync(predicate);
        }

        public async Task<List<LoanApplication>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.LoanApplications
                .Include(l => l.FinancialDetails)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LoanApplication>> GetPendingApplicationsAsync()
        {
            return await _dbContext.LoanApplications
                .Include(l => l.FinancialDetails)
                .Where(l => l.Status == LoanStatusEnum.Submitted || l.Status == LoanStatusEnum.UnderReview)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LoanApplication>> GetAllLoansAsync()
        {
            return await _dbContext.LoanApplications
                .Include(l => l.FinancialDetails)
                .ToListAsync();
        }

        public async Task<bool> ExistsRecentApplicationAsync(Guid userId, DateTime sinceDate)
        {
            return await _dbContext.LoanApplications
                .AnyAsync(l => l.UserId == userId && l.CreatedAt >= sinceDate);
        }

        public IUnitOfWork UnitOfWork => _dbContext;
    }
}
