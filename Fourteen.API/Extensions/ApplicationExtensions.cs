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

            app.UseCors();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();

            app.MapControllers();

            return app;
        }
    }

}
