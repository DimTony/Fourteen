using Fourteen.API.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

namespace Fourteen.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static WebApplication UseApiMiddleware(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fourteen API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseForwardedHeaders();
            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseRateLimiter();
            app.UseCors();

            app.UseMiddleware<ApiVersionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<CsrfValidationMiddleware>();

            app.MapControllers();
            return app;
        }
    }

}
