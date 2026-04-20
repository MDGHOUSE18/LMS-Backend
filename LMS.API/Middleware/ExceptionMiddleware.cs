using LMS.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace LMS.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger<ExceptionMiddleware> logger)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string errorCode;
            string message;

            // ✅ Handle custom domain exceptions
            if (ex is DomainException domainEx)
            {
                statusCode = domainEx.HttpStatusCode;
                errorCode = domainEx.ErrorCode;
                message = domainEx.Message;

                logger.LogWarning($"Domain Exception - Code: {errorCode}, Status: {statusCode}, Message: {message}");
            }
            // ✅ Handle validation exceptions with detailed errors
            else if (ex is ValidationException validationEx)
            {
                statusCode = validationEx.HttpStatusCode;
                errorCode = validationEx.ErrorCode;
                message = validationEx.Message;

                context.Response.StatusCode = statusCode;

                var validationResponse = new
                {
                    statusCode,
                    errorCode,
                    message,
                    data = (object?)null,
                    timestamp = DateTime.UtcNow,
                    errors = validationEx.Errors
                };

                return context.Response.WriteAsync(JsonSerializer.Serialize(validationResponse));
            }
            // ✅ Handle generic exceptions
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                errorCode = "INTERNAL_ERROR";
                message = "An unexpected error occurred. Please try again later.";

                logger.LogError(ex, "Unexpected exception");
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                statusCode,
                errorCode,
                message,
                data = (object?)null,
                timestamp = DateTime.UtcNow
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
