using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fourteen.API.Middleware
{
    public class ApiVersionMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                if (!ctx.Request.Headers.TryGetValue("X-API-Version", out var version) 
                    || version != "1")
                {
                    ctx.Response.StatusCode = 400;
                    await ctx.Response.WriteAsJsonAsync(new { 
                        status = "error", 
                        message = "API version header required" 
                    });
                    return;
                }
            }
            await next(ctx);
        }
    }

    public class ApiVersionHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-Version",
                In = ParameterLocation.Header,
                Required = true,
                Description = "API version. Required header for all API requests.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("1")
                }
            });
        }
    }
}