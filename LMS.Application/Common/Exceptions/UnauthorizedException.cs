using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.common.Exceptions
{
    /// <summary>
    /// Thrown when authentication fails (invalid credentials, expired token, etc.)
    /// HTTP Status: 401 Unauthorized
    /// </summary>
    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException(
            string message = "Unauthorized access",
            string errorCode = "UNAUTHORIZED")
            : base(message, errorCode, 401)
        {
        }
    }
}
