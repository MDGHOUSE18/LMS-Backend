using LMS.Domain.Entities.Auth;
using LMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(User user, OtpPurpose purpose);
        Task<int?> GetOtpAttemptsAsync(int userId, OtpPurpose login);
        Task<bool> VerifyOtpAsync(User user, string otp, OtpPurpose purpose);
        Task<OtpRequest> GetActiveOtpAsync(int userId, OtpPurpose purpose);
    }
}
