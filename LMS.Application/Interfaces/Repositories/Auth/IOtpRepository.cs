using LMS.Domain.Entities.Auth;
using LMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories
{
    public interface IOtpRepository
    {
        Task AddAsync(OtpRequest otp);
        Task<OtpRequest?> GetActiveOtpAsync(int userId, OtpPurpose purpose);
        Task<int?> GetOtpAttemptsAsync(int userId, OtpPurpose login);
        Task UpdateAsync(OtpRequest otp);
    }
}
