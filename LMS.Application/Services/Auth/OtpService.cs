using LMS.Application.common;
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
                UserId = user.Id,
                Purpose = OtpPurpose.Login.ToString(),
                OTPHash = hashedOtp,
                MobileNumber = user.Mobile,

                AttemptCount = 0,
                MaxAttempts = 5,

                Isused = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };
            await _otpRepository.AddAsync(otpRequest);
            string maskedMobile = MaskMobileNumber(user.Mobile);

            return otp.ToString();
        }
        public async Task<OtpRequest> GetActiveOtpAsync(int userId,OtpPurpose purpose)
        {
            return await _otpRepository.GetActiveOtpAsync(userId, purpose);
        }
        public async Task<bool> VerifyOtpAsync(User user, string otp, OtpPurpose purpose)
        {
            var otpRequest = await _otpRepository.GetActiveOtpAsync(user.Id, purpose);
            if (otpRequest==null)
            {
                throw new Exception("OTP has expired. Please request a new OTP.");
            }
            if (otpRequest.AttemptCount >= otpRequest.MaxAttempts)
            {
                throw new Exception("Too many incorrect OTP attempts. Please try again after 15 minutes.");
            }
            if (otpRequest.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("OTP has expired. Please request a new OTP.");
            }
            if (otpRequest.Isused)
            {
                throw new Exception("OTP has already been used. Please request a new OTP.");
            }
            bool isValid = VerifyHash(otp, otpRequest.OTPHash);
            if (isValid)
            {
                otpRequest.Isused = true;
                otpRequest.VerifiedAt = DateTime.UtcNow;
                await _otpRepository.UpdateAsync(otpRequest);
                return true;
            }
            otpRequest.AttemptCount += 1;
            await _otpRepository.UpdateAsync(otpRequest);
            return false;
        }
        public async Task<int?> GetOtpAttemptsAsync(int userId, OtpPurpose login)
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
