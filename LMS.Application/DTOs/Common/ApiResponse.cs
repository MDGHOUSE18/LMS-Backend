using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs.Common
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
