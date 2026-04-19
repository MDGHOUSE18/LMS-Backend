using LMS.Application.Interfaces.Repositories;
using LMS.Domain.Entities.Auth;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Auth
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly LMSDbContext _dbContext;
        public PasswordResetTokenRepository(LMSDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(PasswordResetToken token)
        {
            _dbContext.PasswordResetTokens.Add(token);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string tokenHash)
        {
            return await _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task UpdateAsync(PasswordResetToken token)
        {
            _dbContext.Update(token);
            await _dbContext.SaveChangesAsync();
        }
    }
}
