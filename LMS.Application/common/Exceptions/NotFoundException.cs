using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.common.Exceptions
{
    /// <summary>
    /// Thrown when a resource is not found
    /// HTTP Status: 404 Not Found
    /// </summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(
            string resourceName,
            object key = null,
            string errorCode = "NOT_FOUND")
            : base(
                key != null
                    ? $"{resourceName} with key '{key}' not found."
                    : $"{resourceName} not found.",
                errorCode,
                404)
        {
        }
    }
}
