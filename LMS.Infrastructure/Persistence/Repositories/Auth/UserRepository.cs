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
    public class UserRepository : IUserRepository
    {
        private readonly LMSDbContext _dbContext;
        public UserRepository(LMSDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateAsync(User user)
        {
            _dbContext.Users.Add(user);
            
           await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
