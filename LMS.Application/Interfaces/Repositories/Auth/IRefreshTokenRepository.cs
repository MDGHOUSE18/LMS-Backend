using LMS.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(UserRefreshToken token);

        Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash);

        Task RevokeUserTokensAsync(int userId);

        Task RevokeTokenAsync(UserRefreshToken token);
    }
}
