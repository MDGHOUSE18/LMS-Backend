using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories
{
    public interface INotificationService
    {
        Task SendEmailVerificationAsync(string toEmail, string resetLink);
        Task SendOtpAsync(string toEmail, string otp);
        Task SendPasswordResetLinkAsync(string toEmail, string resetLink);
    }
}
