using FluentValidation;
using Fourteen.Application;
using Fourteen.Application.Common.Behaviors;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace Fourteen.API.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddApiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });


            services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

            services.AddControllers()
                .AddJsonOptions(option =>
                {
                    option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fourteen API",
                    Version = "v1",
                    Description = "External Integration API",
                });

            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            services.AddHealthChecks();

            return services;
        }
    }
}
