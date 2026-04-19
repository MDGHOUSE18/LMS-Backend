using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Auth
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Mobile { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public int RoleId { get; set; }
        public bool IsActive { get; set; } 
        public DateTime CreatedAt { get; set; } 
        public DateTime? LastModifiedAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }

        public virtual Role? Role { get; set; }
        public virtual ICollection<OtpRequest>? OtpRequests { get; set; }
        public virtual ICollection<UserRefreshToken>? UserRefreshTokens { get; set; }
        public virtual ICollection<UserLoginHistory>? UserLoginHistories { get; set; }
        public virtual ICollection<PasswordResetToken>? PasswordResetTokens { get; set; }
    }
}
