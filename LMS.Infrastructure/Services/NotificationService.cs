using LMS.Application.common;
using LMS.Application.Interfaces.Repositories;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly EmailSettings _emailSettings;
        public NotificationService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public async Task SendEmailVerificationAsync(string toEmail, string resetLink)
        {
            var subject = "Verify Your Email";
            var body = $"Click the link toverify your email:\n{resetLink}\n\nThis link expires in 10 minutes.";

            await SendEmailAsync("m.ghouse@amtpl.com", subject, body);
        }
        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var subject = "Your OTP Code";
            var body = $"Your OTP is: {otp}. It will expire in 5 minutes.";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetLinkAsync(string toEmail, string resetLink)
        {
            var subject = "Reset Your Password";
            var body = $"Click the link to reset your password:\n{resetLink}\n\nThis link expires in 10 minutes.";

            await SendEmailAsync("m.ghouse@amtpl.com", subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}
