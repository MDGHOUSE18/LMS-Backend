using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Handlers
{
    public interface ILogService
    {
        void LogInfo(string message, object? data = null);
        void LogWarning(string message, object? data = null);
        void LogError(string message, Exception ex, object? data = null);
    }
}
