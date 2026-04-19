using LMS.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories
{
    public interface IEmailVerificationRepository
    {
        Task AddAsync(EmailVerificationToken tokenEntity);
        Task<EmailVerificationToken> GetValidTokenAsync(string tokenHash);
        Task UpdateAsync(EmailVerificationToken tokenEntity);
        Task InvalidateOldTokensAsync(int userId);
    }
}
