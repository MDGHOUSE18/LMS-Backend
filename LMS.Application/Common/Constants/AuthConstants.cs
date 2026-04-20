using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Common.Constants
{
    public static class AuthMessages
    {
        // Success Messages
        public const string USER_REGISTERED_SUCCESSFULLY = "User created successfully";
        public const string OTP_SENT_SUCCESSFULLY = "OTP sent successfully to your registered email";
        public const string OTP_VERIFIED_SUCCESSFULLY = "OTP verified successfully";
        public const string TOKEN_REFRESHED_SUCCESSFULLY = "Token refreshed successfully";
        public const string USER_LOGGED_OUT_SUCCESSFULLY = "User logged out successfully";
        public const string PASSWORD_RESET_LINK_SENT = "Password reset link sent";
        public const string PASSWORD_RESET_SUCCESSFULLY = "Password reset successfully";

        // Error Messages
        public const string INVALID_CREDENTIALS = "Invalid credentials";
        public const string INVALID_PASSWORD = "Invalid password. Please enter the correct password or reset it if you have forgotten it.";
        public const string ACCOUNT_INACTIVE = "Your account is currently inactive. Please reach out to the administrator or support team for activation.";
        public const string ACCOUNT_LOCKED = "Your account is locked. Try again in {0} minutes.";
        public const string INVALID_OTP = "Invalid OTP. Please enter a valid OTP.";
        public const string TOO_MANY_OTP_ATTEMPTS = "Too many invalid OTP attempts. Your account is locked for 15 minutes.";
        public const string EMAIL_ALREADY_REGISTERED = "Email is already registered. Please use a different email.";
        public const string INVALID_SESSION = "Invalid session. Please log in again.";
        public const string USER_ACCOUNT_INACTIVE = "User account is inactive.";
        public const string USER_ROLE_INVALID = "User role configuration is invalid.";
    }

    public static class AuthConstants
    {
        // Lockout Settings
        public const int LOCKOUT_DURATION_MINUTES = 15;
        public const int MAX_OTP_ATTEMPTS = 5;

        // Token Settings
        public const int REFRESH_TOKEN_EXPIRY_DAYS = 7;
        public const int ACCESS_TOKEN_EXPIRY_HOURS = 1;
        public const int PASSWORD_RESET_TOKEN_EXPIRY_MINUTES = 10;

        // OTP Settings
        public const int OTP_EXPIRY_MINUTES = 5;
    }
}
