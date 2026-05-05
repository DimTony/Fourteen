using System.Text;

namespace Fourteen.API.Middleware
{
    public class ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            // Only log response bodies for auth endpoints with error status codes
            if ((ctx.Request.Path.StartsWithSegments("/auth") || ctx.Request.Path.StartsWithSegments("/Auth")) &&
                ctx.Response.StatusCode >= 400)
            {
                // Capture the original response stream
                var originalBodyStream = ctx.Response.Body;
                using var responseBody = new MemoryStream();
                ctx.Response.Body = responseBody;

                try
                {
                    await next(ctx);

                    // Read the response body
                    ctx.Response.Body.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(ctx.Response.Body);
                    var body = await reader.ReadToEndAsync();
                    ctx.Response.Body.Seek(0, SeekOrigin.Begin);

                    logger.LogWarning(
                        "❌ Auth endpoint error response: {Method} {Path} returned {Status}. Body: {Body}",
                        ctx.Request.Method,
                        ctx.Request.Path,
                        ctx.Response.StatusCode,
                        body);

                    // Copy to original stream
                    await responseBody.CopyToAsync(originalBodyStream);
                }
                finally
                {
                    ctx.Response.Body = originalBodyStream;
                }
            }
            else
            {
                await next(ctx);
            }
        }
    }
}
