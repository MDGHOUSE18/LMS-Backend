using LMS.Application.Interfaces.Repositories;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Enums;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Auth
{
    public class OtpRepository : IOtpRepository
    {
        private readonly LMSDbContext _dbContext;
        public OtpRepository(LMSDbContext dbContext)
        {
            _dbContext = dbContext; 
        }
        public async Task AddAsync(OtpRequest otp)
        {
            _dbContext.OtpRequests.Add(otp);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<OtpRequest?> GetActiveOtpAsync(Guid userId, OtpPurpose purpose)
        {
            return await _dbContext.OtpRequests
                .Where(o => o.UserId == userId && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.ExpiresAt)
                .FirstOrDefaultAsync();
        }

        public Task<int?> GetOtpAttemptsAsync(Guid userId, OtpPurpose login)
        {
            return Task.FromResult<int?>(null);
        }

        public async Task UpdateAsync(OtpRequest otp)
        {
            _dbContext.OtpRequests.Update(otp);
            await _dbContext.SaveChangesAsync();
        }
    }
}
