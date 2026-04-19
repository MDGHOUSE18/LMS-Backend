using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Auth
{
    public class OtpRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OTPHash { get; set; }
        public string Purpose { get; set; }
        public string MobileNumber {  get; set; }
        public int AttemptCount { get; set; }
        public int MaxAttempts { get; set; }
        public bool Isused { get; set; }
        public DateTime ExpiresAt {  get; set; }
        public DateTime CreatedAt {  get; set; }
        public DateTime VerifiedAt {  get; set; }

    }
}
