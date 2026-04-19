using LMS.Application.Interfaces.Repositories;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Auth
{
    public class RoleRepository : IRoleRepository
    {
        private readonly LMSDbContext _dbContext;
        public RoleRepository(LMSDbContext context)
        {
            _dbContext = context;
        }
        public async Task<string?> GetRoleNameByIdAsync(int roleId)
        {
            return await _dbContext.Roles.Where(r => r.Id == roleId).Select(r => r.RoleName).FirstOrDefaultAsync();
        }

        public  int GetDefaultRole()
        {
            return _dbContext.Roles.Where(r=>r.RoleName== "Customer").Select(r=>r.Id).FirstOrDefault();
        }
    }
}
