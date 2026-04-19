using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Auth
{
    public class UserLoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime{ get; set; }
        public DateTime? LogoutTime{ get; set; }
        public string? IpAddress{ get; set; }
        public string? UserAgent{ get; set; }
        public bool IsSuccess { get; set; }
        public string? FailureReason { get; set; }

    }
}
