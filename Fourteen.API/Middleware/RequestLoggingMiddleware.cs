
using System.Diagnostics;

namespace Fourteen.API.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            var sw = Stopwatch.StartNew();
            await next(ctx);
            sw.Stop();
            logger.LogInformation("{Method} {Path} {Status} {ElapsedMs}ms",
                ctx.Request.Method, ctx.Request.Path,
                ctx.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}