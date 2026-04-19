using LMS.Application.Interfaces.Repositories;
using LMS.Domain.Entities.Auth;
using LMS.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Auth
{
    public class UserLoginHistoryRepository : IUserLoginHistoryRepository
    {
        private readonly LMSDbContext _dbCOntext;
        public UserLoginHistoryRepository(LMSDbContext dbContext)
        {
            _dbCOntext = dbContext;
        }
        public async Task AddAsync(UserLoginHistory history)
            {
            _dbCOntext.UserLoginHistory.Add(history);

            await _dbCOntext.SaveChangesAsync();
        }
    }
}
