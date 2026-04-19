namespace LMS.API.Middleware
{
    public class AuthRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthRateLimitMiddleware> _logger;

        // ✅ In-memory store for rate limit attempts
        private static readonly Dictionary<string, (int attempts, DateTime resetTime)> _rateLimitStore =
            new Dictionary<string, (int, DateTime)>();
        private static readonly object _lockObject = new object();

        public AuthRateLimitMiddleware(RequestDelegate next, ILogger<AuthRateLimitMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // ✅ Apply rate limiting only to auth endpoints
            if (path.Contains("/api/auth"))
            {
                bool isLimited = false;

                if (path.Contains("/login"))
                {
                    isLimited = await CheckRateLimit(context, ipAddress, "login", 5, 60);
                }
                else if (path.Contains("/verify-otp"))
                {
                    isLimited = await CheckRateLimit(context, ipAddress, "otp", 5, 60);
                }
                else if (path.Contains("/forgot-password"))
                {
                    isLimited = await CheckRateLimit(context, ipAddress, "password-reset", 3, 300);
                }
                else if (path.Contains("/reset-password"))
                {
                    isLimited = await CheckRateLimit(context, ipAddress, "password-reset", 3, 300);
                }
                else if (path.Contains("/refresh-token"))
                {
                    isLimited = await CheckRateLimit(context, ipAddress, "refresh", 10, 60);
                }

                // ✅ If rate limited, return 429 response
                if (isLimited)
                {
                    _logger.LogWarning($"Rate limit exceeded for IP: {ipAddress}, Path: {path}");
                    return;
                }
            }

            await _next(context);
        }

        private async Task<bool> CheckRateLimit(
            HttpContext context,
            string ipAddress,
            string endpoint,
            int maxAttempts,
            int windowSeconds)
        {
            var key = $"{endpoint}:{ipAddress}";
            var now = DateTime.UtcNow;

            lock (_lockObject)
            {
                if (_rateLimitStore.TryGetValue(key, out var stored))
                {
                    // ✅ Check if window has expired
                    if (now > stored.resetTime)
                    {
                        // Window expired, reset counter
                        _rateLimitStore[key] = (1, now.AddSeconds(windowSeconds));
                        return false;
                    }

                    // ✅ Check if exceeded limit
                    if (stored.attempts >= maxAttempts)
                    {
                        context.Response.StatusCode = 429;
                        context.Response.ContentType = "application/json";

                        var remainingSeconds = (int)(stored.resetTime - now).TotalSeconds;

                        context.Response.WriteAsJsonAsync(new
                        {
                            statusCode = 429,
                            errorCode = "RATE_LIMIT_EXCEEDED",
                            message = $"Too many requests. Please try again in {remainingSeconds} seconds.",
                            data = (object?)null,
                            timestamp = DateTime.UtcNow
                        }).Wait();

                        return true;
                    }

                    // ✅ Increment attempts
                    _rateLimitStore[key] = (stored.attempts + 1, stored.resetTime);
                    return false;
                }

                // ✅ First attempt - create entry
                _rateLimitStore[key] = (1, now.AddSeconds(windowSeconds));
                return false;
            }
        }
    }
}
