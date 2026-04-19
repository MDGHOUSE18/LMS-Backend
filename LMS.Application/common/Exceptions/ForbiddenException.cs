using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.common.Exceptions
{
    /// <summary>
    /// Thrown when user is authenticated but doesn't have permission (account inactive, locked, etc.)
    /// HTTP Status: 403 Forbidden
    /// </summary>
    public class ForbiddenException : DomainException
    {
        public ForbiddenException(
            string message = "Access forbidden",
            string errorCode = "FORBIDDEN")
            : base(message, errorCode, 403)
        {
        }
    }
}
