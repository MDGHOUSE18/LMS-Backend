using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<LoanApplication> CreateAsync(LoanApplication loan)
        {
            _dbContext.LoanApplications.Add(loan);
            await _dbContext.SaveChangesAsync();
            return loan;
        }

        public async Task<LoanApplication?> GetByIdAsync(int id)
        {
            return await _dbContext.LoanApplications.FindAsync(id);
        }

        public Task UpdateAsync(LoanApplication loan)
        {
            _dbContext.LoanApplications.Update(loan);
            return _dbContext.SaveChangesAsync();
        }
    }
}
