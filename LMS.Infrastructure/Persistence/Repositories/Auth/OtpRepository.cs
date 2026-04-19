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

        public async Task<OtpRequest?> GetActiveOtpAsync(int userId, OtpPurpose purpose)
        {
            return await _dbContext.OtpRequests
                .Where(o => o.UserId == userId
                         && o.Purpose == purpose.ToString()
                         && !o.Isused
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o=>o.Id)
                .FirstOrDefaultAsync();
        }

        public Task<int?> GetOtpAttemptsAsync(int userId, OtpPurpose login)
        {
            return _dbContext.OtpRequests
                .Where(o => o.UserId == userId && o.Purpose == login.ToString())
                .Select(o => (int?)o.AttemptCount)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(OtpRequest otp)
        {
            _dbContext.OtpRequests.Update(otp);
            await _dbContext.SaveChangesAsync();
        }
    }
}
