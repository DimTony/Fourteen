using Fourteen.Application.Common.DTOs;
using Fourteen.Domain.Exceptions;
using System.Text.Json;

namespace Fourteen.API.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (InvalidNameException ex)
            {
                _logger.LogWarning(ex, "Invalid name input");
                await WriteError(ctx, 422, ex.Message);
            }
            catch (NoPredictionException ex)
            {
                _logger.LogInformation(ex, "No prediction available");
                await WriteError(ctx, 200, ex.Message);
            }
            catch (UnparsableQueryException ex)
            {
                _logger.LogInformation(ex, "Try: 'young males', 'adult females from Kenya'");
                await WriteError(ctx, 400, ex.Message);
            }
            catch (UnauthorizedException ex)         
            {
                _logger.LogWarning(ex, "Unauthorized");
                await WriteError(ctx, 401, ex.Message);
            }
            catch (ForbiddenException ex)            
            {
                _logger.LogWarning(ex, "Forbidden");
                await WriteError(ctx, 403, ex.Message);
            }
            catch (UpstreamApiException ex)
            {
                var status = ex.StatusCode is 502 or null ? 502 : 500;
                _logger.LogError(ex, "Upstream failure");
                await WriteError(ctx, status, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteError(ctx, 500, "Upstream or server failure");
            }
        }

        private static Task WriteError(HttpContext ctx, int status, string message)
        {
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";

            var errorResponse = new ApiErrorResponse
            {
                Status = "error",
                Message = message
            };

            var body = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return ctx.Response.WriteAsync(body);
        }
    }
}
