using LMS.Application.Common.Constants;
using LMS.Application.Common.Exceptions;
using LMS.Application.DTOs.Auth;
using LMS.Application.DTOs.Common;
using LMS.Application.Interfaces.Repositories;
using LMS.Application.Interfaces.Security;
using LMS.Application.Interfaces.Services;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LMS.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUserLoginHistoryRepository _userLoginHistoryRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IOtpService _otpService;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly INotificationService _notificationService;
        private readonly IEmailVerificationRepository _emailVerificationRepository;

        public AuthService(IUserRepository userRepository, IOtpService otpService,
            ITokenService tokenService, IUserLoginHistoryRepository userLoginHistoryRepository,
            IRefreshTokenRepository refreshTokenRepository, IRoleRepository roleRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository, INotificationService notificationService,
            IEmailVerificationRepository emailVerificationRepository
            )
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _userLoginHistoryRepository = userLoginHistoryRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _roleRepository = roleRepository;
            _otpService = otpService;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _notificationService = notificationService;
            _emailVerificationRepository = emailVerificationRepository;
        }

        public async Task<string> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new ConflictException(AuthMessages.EMAIL_ALREADY_REGISTERED);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var defaultRoleId = _roleRepository.GetDefaultRole();

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Mobile = request.MobileNumber,
                PasswordHash = hashedPassword,
                RoleId = defaultRoleId,
                IsActive = true,
                IsEmailVerified = false, // 🔴 Important
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            // 🔐 Generate verification token
            var plainToken = GenerateSecureToken();
            var tokenHash = _tokenService.HashToken(plainToken);

            var emailToken = new EmailVerificationToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _emailVerificationRepository.AddAsync(emailToken);

            // 🔗 Create verification link
            var verifyLink = $"https://yourapp.com/verify-email?token={plainToken}";

            // 📧 Send email
            await _notificationService.SendEmailVerificationAsync(user.Email, verifyLink);

            // ⚠️ TEMP (remove in production)
            return $"User registered successfully. Verification token: {plainToken}";
        }
        public async Task<string> ResendVerificationEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException("Email is required.");
            }

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                throw new NotFoundException("User", email);
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("Your account has been deactivated. Please contact support.");
            }

            if (user.IsEmailVerified)
            {
                return "Email is already verified.";
            }

            // 🔴 OPTIONAL BUT RECOMMENDED
            // Invalidate old tokens to avoid multiple valid tokens
            await _emailVerificationRepository.InvalidateOldTokensAsync(user.Id);

            // 🔐 Generate new token
            var plainToken = GenerateSecureToken();
            var tokenHash = _tokenService.HashToken(plainToken);

            var emailToken = new EmailVerificationToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _emailVerificationRepository.AddAsync(emailToken);

            // 🔗 Create verification link
            var verifyLink = $"https://yourapp.com/verify-email?token={plainToken}";

            // 📧 Send email
            await _notificationService.SendEmailVerificationAsync(user.Email, verifyLink);

            // ⚠️ TEMP (remove in production)
            return $"Verification email sent. Token: {plainToken}";
        }
        public async Task<string> VerifyEmailAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ValidationException("Invalid verification request.");
            }

            var tokenHash = _tokenService.HashToken(token);

            var tokenEntity = await _emailVerificationRepository.GetValidTokenAsync(tokenHash);

            if (tokenEntity == null)
            {
                throw new ValidationException("Invalid or expired verification token.");
            }

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);

            if (user == null)
            {
                throw new NotFoundException("User", tokenEntity.UserId);
            }

            if (user.IsEmailVerified)
            {
                return "Email already verified.";
            }

            // ✅ Mark user as verified
            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.LastModifiedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // ✅ Mark token as used
            tokenEntity.IsUsed = true;
            tokenEntity.VerifiedAt = DateTime.UtcNow;

            await _emailVerificationRepository.UpdateAsync(tokenEntity);

            return "Email verified successfully.";
        }

        public async Task<string> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent)
        {
            User user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new ConflictException("Email is already registered. Please use a different email.");
            }
            if (user.IsActive == false)
            {
                await _userLoginHistoryRepository.AddAsync(new UserLoginHistory
                {
                    UserId = user.Id,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccess = false,
                    FailureReason = "Account inactive"
                });
                throw new ForbiddenException("Your account is currently inactive. Please reach out to the administrator or support team for activation.");
            }
            if (user.IsLocked)
            {
                if (user.LockoutEnd <= DateTime.UtcNow)
                {
                    user.IsLocked = false;
                    user.LockoutEnd = null;
                    user.FailedLoginAttempts = 0;
                    await _userRepository.UpdateAsync(user);
                }
                else
                {
                    var remainingTime = user.LockoutEnd.Value - DateTime.UtcNow;
                    throw new ForbiddenException(
                        $"Your account is locked. Try again in {remainingTime.Minutes} minutes."
                    );
                }
            }
            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid)
            {
                await _userLoginHistoryRepository.AddAsync(new UserLoginHistory
                {
                    UserId = user.Id,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccess = false,
                    FailureReason = "Invalid password"
                });
                throw new UnauthorizedException("Invalid password. Please enter the correct password or reset it if you have forgotten it.");
            }

            await _refreshTokenRepository.RevokeUserTokensAsync(user.Id);

            var otp = await _otpService.GenerateOtpAsync(user, OtpPurpose.Login);
            await _notificationService.SendOtpAsync(user.Email, otp);
            return "OTP send send successfullt to registered email number";

        }
        public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request, string ipAddress, string userAgent)
        {
            User user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedException(AuthMessages.INVALID_CREDENTIALS);
            }
            //Check if user is locked
            //→ If locked → return error
            bool isValid = await _otpService.VerifyOtpAsync(user, request.Otp, OtpPurpose.Login);
            if (!isValid)
            {
                var otpAttempts = await _otpService.GetOtpAttemptsAsync(user.Id, OtpPurpose.Login);
                if (otpAttempts == 5)
                {
                    user.IsLocked = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = otpAttempts ?? 0;

                    await _userRepository.UpdateAsync(user);
                }
                await _userLoginHistoryRepository.AddAsync(new UserLoginHistory
                {
                    UserId = user.Id,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccess = false,
                    FailureReason = "Too many attempts of invlaid otp. Account is locking in 15 min"
                });
                throw new ValidationException(
                    otpAttempts >= 5
                        ? "Too many invalid OTP attempts. Your account is locked for 15 minutes."
                        : "Invalid OTP. Please enter a valid OTP."
                );
            }
            var roleName = await _roleRepository.GetRoleNameByIdAsync(user.RoleId);

            if (string.IsNullOrEmpty(roleName))
            {
                throw new UnauthorizedException("User role configuration is invalid.");
            }
            var accessToken = _tokenService.GenerateAccessToken(user, roleName);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = _tokenService.HashToken(refreshToken);

            await _refreshTokenRepository.CreateAsync(new UserRefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            });

            await _userLoginHistoryRepository.AddAsync(new UserLoginHistory
            {
                UserId = user.Id,
                LoginTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccess = true,
            });

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roleName
                },
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new UnauthorizedException("Invalid session. Please log in again.");
            }
            var tokenHash = _tokenService.HashToken(refreshToken);

            UserRefreshToken savedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (savedToken == null)
            {
                throw new UnauthorizedException("Invalid session. Please log in again.");
            }

            if (savedToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new UnauthorizedException("Invalid session. Please log in again.");
            }

            if (savedToken.RevokedAt != null && savedToken.RevokedAt != DateTime.MinValue)
            {
                throw new UnauthorizedException("Invalid session. Please log in again.");
            }
            User user = await _userRepository.GetByIdAsync(savedToken.UserId);
            if (user == null)
            {
                throw new UnauthorizedException("Invalid session. Please log in again.");
            }
            if (!user.IsActive)
            {
                throw new ForbiddenException("User account is inactive.");
            }
            var roleName = await _roleRepository.GetRoleNameByIdAsync(user.RoleId);
            if (string.IsNullOrEmpty(roleName))
            {
                throw new UnauthorizedException("User role configuration is invalid.");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user, roleName);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newRefreshTokenHash = _tokenService.HashToken(newRefreshToken);

            savedToken.IsRevoked = true;
            savedToken.RevokedAt = DateTime.UtcNow;
            savedToken.ReplacedByTokenHash = newRefreshTokenHash;
            await _refreshTokenRepository.RevokeTokenAsync(savedToken);

            await _refreshTokenRepository.CreateAsync(new UserRefreshToken
            {
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            });

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = roleName
                }
            };

        }

        public async Task LogoutAsync(int userId)
        {
            await _refreshTokenRepository.RevokeUserTokensAsync(userId);
        }

        public async Task<string> ForgotPasswordAsync(string Email)
        {
            User user = await _userRepository.GetByEmailAsync(Email);

            if (user == null)
            {
                throw new NotFoundException("User", Email);
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("Your account has been deactivated. Please contact support.");
            }

            var plainToken = GenerateSecureToken();
            var tokenHash = _tokenService.HashToken(plainToken);

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetTokenRepository.AddAsync(resetToken);
            var resetLink = $"https://yourapp.com/reset-password?token={plainToken}";

            await _notificationService.SendPasswordResetLinkAsync(user.Email, resetLink);
            // ⚠️ TEMP: return token (for testing only)
            return plainToken;

        }

        public async Task<string> ResetPasswordAsync(ForgotPasswordRequest request)
        {
            var tokenHash = _tokenService.HashToken(request.Token);
            var tokenEntity = await _passwordResetTokenRepository.GetValidTokenAsync(tokenHash);
            if (tokenEntity == null)
            {
                throw new ValidationException("Invalid or expired reset token");
            }

            // ✅ Get user
            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);

            if (user == null)
            {
                throw new NotFoundException("User", tokenEntity.UserId);
            }


            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.LastModifiedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            tokenEntity.IsUsed = true;
            await _passwordResetTokenRepository.UpdateAsync(tokenEntity);

            await _refreshTokenRepository.RevokeUserTokensAsync(user.Id);

            return "Password reset successful";
        }

        private string GenerateSecureToken()
        {
            byte[] bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }
    }
}
