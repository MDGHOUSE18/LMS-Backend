using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when there's a conflict (duplicate email, duplicate resource, etc.)
    /// HTTP Status: 409 Conflict
    /// </summary>
    public class ConflictException : DomainException
    {
        public ConflictException(
            string message = "Resource conflict",
            string errorCode = "CONFLICT")
            : base(message, errorCode, 409)
        {
        }
    }
}
