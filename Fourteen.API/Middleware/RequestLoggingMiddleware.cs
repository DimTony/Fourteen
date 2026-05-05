
using System.Diagnostics;
using System.Text;

namespace Fourteen.API.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            var sw = Stopwatch.StartNew();
            
            // Log request details for POST endpoints
            if (ctx.Request.Method == "POST")
            {
                await LogPostRequest(ctx);
            }
            
            await next(ctx);
            
            sw.Stop();
            
            // Log response and timing
            logger.LogInformation(
                "{Method} {Path} {Status} {ElapsedMs}ms",
                ctx.Request.Method, 
                ctx.Request.Path,
                ctx.Response.StatusCode, 
                sw.ElapsedMilliseconds);

            // Log errors with status codes 400-599
            if (ctx.Response.StatusCode >= 400)
            {
                logger.LogWarning(
                    "Error response: {Method} {Path} returned {Status} in {ElapsedMs}ms. " +
                    "Content-Type: {ContentType}, Content-Length: {ContentLength}",
                    ctx.Request.Method,
                    ctx.Request.Path,
                    ctx.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    ctx.Request.ContentType,
                    ctx.Request.ContentLength);
            }
        }

        private async Task LogPostRequest(HttpContext ctx)
        {
            var path = ctx.Request.Path;
            var contentType = ctx.Request.ContentType ?? "not-specified";
            var contentLength = ctx.Request.ContentLength ?? -1;

            logger.LogInformation(
                "POST {Path} | Content-Type: {ContentType}, Content-Length: {ContentLength}",
                path,
                contentType,
                contentLength);

            // For /auth/google/callback specifically, log the body content
            if (path.StartsWithSegments("/auth/google/callback"))
            {
                if (contentLength == 0)
                {
                    logger.LogWarning(
                        "⚠️  POST {Path}: Request body is EMPTY (Content-Length: 0)",
                        path);
                    return;
                }

                if (contentLength == -1)
                {
                    logger.LogWarning(
                        "⚠️  POST {Path}: Content-Length header missing or -1",
                        path);
                }

                if (!contentType.Contains("application/json"))
                {
                    logger.LogWarning(
                        "⚠️  POST {Path}: Content-Type is {ContentType}, expected application/json",
                        path,
                        contentType);
                }

                try
                {
                    // Enable buffering so request body can be read multiple times
                    ctx.Request.EnableBuffering();

                    // Read request body
                    using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();

                    // Reset stream for the controller to read
                    ctx.Request.Body.Position = 0;

                    if (string.IsNullOrWhiteSpace(body))
                    {
                        logger.LogWarning(
                            "⚠️  POST {Path}: Request body is null or whitespace",
                            path);
                    }
                    else
                    {
                        var bodyPreview = body.Length > 100 ? body.Substring(0, 100) + "..." : body;
                        logger.LogInformation(
                            "POST {Path} body ({Length} chars): {Body}",
                            path,
                            body.Length,
                            bodyPreview);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "❌ Error reading request body for POST {Path}",
                        path);
                }
            }
        }
    }
}