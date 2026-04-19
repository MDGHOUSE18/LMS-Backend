using LMS.Application.Interfaces.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Services
{
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;
        }

        public void LogError(string message, Exception ex, object? data = null)
        {
            if (data != null)
                _logger.LogError("{Message} | Data: {@Data}", message, data);
            else
                _logger.LogError(message);
        }

        public void LogInfo(string message, object? data = null)
        {
            if (data != null)
                _logger.LogInformation("{Message} | Data: {@Data}", message, data);
            else
                _logger.LogInformation(message);
        }

        public void LogWarning(string message, object? data = null)
        {
            if (data != null)
                _logger.LogWarning("{Message} | Data: {@Data}", message, data);
            else
                _logger.LogWarning(message);
        }
    }
}
