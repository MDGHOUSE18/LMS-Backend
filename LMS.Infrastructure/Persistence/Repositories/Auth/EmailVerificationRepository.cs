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
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly LMSDbContext _dbContext;
        public EmailVerificationRepository(LMSDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(EmailVerificationToken tokenEntity)
        {
            _dbContext.EmailVerificationTokens.Add(tokenEntity);
            await _dbContext.SaveChangesAsync();    
        }

        public async Task<EmailVerificationToken> GetValidTokenAsync(string tokenHash)
        {
            return await _dbContext.EmailVerificationTokens
                .Where(t => t.TokenHash == tokenHash && t.ExpiresAt > DateTime.UtcNow && !t.IsUsed)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidateOldTokensAsync(int userId)
        {
            var tokens = await _dbContext.EmailVerificationTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
                token.VerifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmailVerificationToken tokenEntity)
        {
            _dbContext.EmailVerificationTokens.Update(tokenEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
