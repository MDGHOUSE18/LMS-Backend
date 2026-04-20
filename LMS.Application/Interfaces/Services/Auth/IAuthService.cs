using LMS.Application.DTOs.Auth;
using LMS.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent);

        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);

        Task LogoutAsync(int userId);

        Task<string> RegisterAsync(RegisterRequestDto request);
        Task<string> ResendVerificationEmailAsync(string email);
        Task<string> VerifyEmailAsync(string token);
        Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request, string ipAddress, string userAgent);
        Task<string> ForgotPasswordAsync(string Email);
        Task<string> ResetPasswordAsync(ForgotPasswordRequest request);

    }
}
