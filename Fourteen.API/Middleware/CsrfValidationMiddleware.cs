using System.Security.Cryptography;
using System.Text.Json;

namespace Fourteen.API.Middleware
{
    public sealed class CsrfValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CsrfValidationMiddleware> _logger;

        public CsrfValidationMiddleware(
            RequestDelegate next,
            ILogger<CsrfValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post ||
                context.Request.Method == HttpMethods.Put ||
                context.Request.Method == HttpMethods.Delete ||
                context.Request.Method == HttpMethods.Patch)
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
                {
                    await _next(context);
                    return;
                }

                var path = context.Request.Path.Value ?? "";
                if (path.StartsWith("/auth", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }
                if (!context.Request.Cookies.ContainsKey("csrf_token"))
                {
                    _logger.LogWarning("CSRF token missing from cookies for {Method} {Path}", 
                        context.Request.Method, path);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = "error",
                        message = "CSRF token missing"
                    });
                    return;
                }

                var cookieToken = context.Request.Cookies["csrf_token"];

                var headerToken = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(headerToken) || string.IsNullOrWhiteSpace(cookieToken))
                {
                    _logger.LogWarning("CSRF token missing from X-CSRF-Token header for {Method} {Path}", 
                        context.Request.Method, path);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = "error",
                        message = "CSRF token missing from headers"
                    });
                    return;
                }

                if (!SecurityExtensions.ConstantTimeEquals(cookieToken, headerToken))
                {
                    _logger.LogWarning("CSRF token mismatch for {Method} {Path}", 
                        context.Request.Method, path);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = "error",
                        message = "CSRF token validation failed"
                    });
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class SecurityExtensions
    {
        public static bool ConstantTimeEquals(string a, string b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a == null || b == null)
                return false;

            byte[] aBytes = System.Text.Encoding.UTF8.GetBytes(a);
            byte[] bBytes = System.Text.Encoding.UTF8.GetBytes(b);

            if (aBytes.Length != bBytes.Length)
                return false;

            int result = 0;
            for (int i = 0; i < aBytes.Length; i++)
            {
                result |= aBytes[i] ^ bBytes[i];
            }

            return result == 0;
        }
    }
}
