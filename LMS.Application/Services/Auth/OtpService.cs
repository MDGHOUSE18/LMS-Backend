using LMS.Application.Common.Settings;
using LMS.Application.Interfaces.Repositories;
using LMS.Application.Interfaces.Services;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace LMS.Application.Services.Auth
{
    public class OtpService : IOtpService
    {
        private readonly string _otpSecret;
        private readonly bool _enableBypass;
        private readonly string _bypassCode;
        private readonly IOtpRepository _otpRepository;
        public OtpService(IOptions<OtpSettings> otpSettings, IOtpRepository otpRepository)
        {
            var settings = otpSettings.Value;
            _otpSecret = settings.SecretKey;
            _enableBypass = settings.EnableBypass;
            _bypassCode = settings.BypassCode;
            _otpRepository = otpRepository;
        }
        public async Task<string> GenerateOtpAsync(User user, OtpPurpose purpose)
        {
            var otp = GenerateSecureOtp();
            var hashedOtp = HashOtp(otp);
            OtpRequest otpRequest = new OtpRequest
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                OTPHash = hashedOtp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };
            await _otpRepository.AddAsync(otpRequest);
            string maskedMobile = MaskMobileNumber(user.Mobile);

            return otp.ToString();
        }
        public async Task<OtpRequest?> GetActiveOtpAsync(Guid userId, OtpPurpose purpose)
        {
            return await _otpRepository.GetActiveOtpAsync(userId, purpose);
        }

        public async Task<bool> VerifyOtpAsync(User user, string otp, OtpPurpose purpose)
        {
            var otpRequest = await _otpRepository.GetActiveOtpAsync(user.Id, purpose);
            if (otpRequest == null || otpRequest.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("OTP has expired. Please request a new OTP.");
            }

            bool isValid = VerifyHash(otp, otpRequest.OTPHash);
            if (isValid)
            {
                await _otpRepository.UpdateAsync(otpRequest);
                return true;
            }

            return false;
        }

        public async Task<int?> GetOtpAttemptsAsync(Guid userId, OtpPurpose login)
        {
            return await _otpRepository.GetOtpAttemptsAsync(userId, login);
        }
        #region Private Methods
        private string GenerateSecureOtp()
        {
            byte[] randomBytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            int value = BitConverter.ToInt32(randomBytes, 0) & int.MaxValue;

            int otp = (value % 900000) + 100000;

            return otp.ToString();
        }
        private string HashOtp(string otp)
        {
            var combined = otp + _otpSecret;

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(combined);
                var hash = sha.ComputeHash(bytes);

                return Convert.ToBase64String(hash);
            }
        }
        private bool VerifyHash(string inputOtp, string storedHash)
        {
            // ✅ DEV BYPASS (only when enabled)
            if (_enableBypass && inputOtp == _bypassCode)
            {
                return true;
            }
            var hashedInput = HashOtp(inputOtp);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hashedInput),
                Encoding.UTF8.GetBytes(storedHash)
            );
        }
        private string MaskMobileNumber(string mobile)
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Length < 4)
                return "****";

            int visibleDigits = 4;
            int maskedLength = mobile.Length - visibleDigits;

            return new string('*', maskedLength) + mobile.Substring(maskedLength);
        }

        #endregion
    }
}
