using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Loan
{
    public class LoanFinancialRepository : ILoanFinancialRepository
    {
        private readonly LMSDbContext _dbContext;
        public LoanFinancialRepository(LMSDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(LoanFinancialDetails details)
        {
            _dbContext.LoanFinancialDetails.Add(details);
            await _dbContext.SaveChangesAsync();
        }
    }
}
