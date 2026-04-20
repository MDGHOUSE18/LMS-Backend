using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Common.Exceptions
{
    /// <summary>
    /// Base exception for all domain/business logic exceptions
    /// </summary>
    public class DomainException : Exception
    {
        public string ErrorCode { get; set; }
        public int HttpStatusCode { get; set; }

        public DomainException(
            string message,
            string errorCode = "DOMAIN_ERROR",
            int httpStatusCode = 500) : base(message)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }
    }
}
