using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when input validation fails (invalid OTP, invalid format, etc.)
    /// HTTP Status: 400 Bad Request
    /// </summary>
    public class ValidationException : DomainException
    {
        public Dictionary<string, string[]> Errors { get; set; }

        public ValidationException(
            string message = "Validation failed",
            string errorCode = "VALIDATION_ERROR",
            Dictionary<string, string[]> errors = null)
            : base(message, errorCode, 400)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
