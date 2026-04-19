using LMS.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories
{
    public interface IUserLoginHistoryRepository
    {
        Task AddAsync(UserLoginHistory history);
    }
}
