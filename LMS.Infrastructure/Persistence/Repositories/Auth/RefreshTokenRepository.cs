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
    public class RefreshTokenRepository : IRefreshTokenRepository
    {

        private readonly LMSDbContext _dbContext;
        public RefreshTokenRepository(LMSDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateAsync(UserRefreshToken token)
        {
            _dbContext.UserRefreshTokens.Add(token);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _dbContext.UserRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task RevokeTokenAsync(UserRefreshToken token)
        {
            _dbContext.UserRefreshTokens.Remove(token);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RevokeUserTokensAsync(Guid userId)
        {
            var tokens = await _dbContext.UserRefreshTokens
                .Where(rt => rt.UserId == userId && rt.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            if (tokens.Count == 0)
            {
                return;
            }

            _dbContext.UserRefreshTokens.RemoveRange(tokens);
            await _dbContext.SaveChangesAsync();
        }
    }
}
