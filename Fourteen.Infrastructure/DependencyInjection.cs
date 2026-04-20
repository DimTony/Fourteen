using Fourteen.Application.Common.Behaviors;
using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Infrastructure.Persistence;
using Fourteen.Infrastructure.Persistence.Repositories;
using Fourteen.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fourteen.Infrastructure;

 public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
               this IServiceCollection services,
               IConfiguration configuration)
        {
            services
                .AddPersistence(configuration)
                .AddRepositories()
                .AddServices(configuration);

            return services;
        }

        private static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        private static IServiceCollection AddRepositories(
           this IServiceCollection services)
        {
            services.AddScoped<IProfileRepository, ProfileRepository>();
         

            return services;
        }


        private static IServiceCollection AddServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IServices, ExternalServices>();
            services.AddScoped<NaturalLanguageQueryParser>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FeatureFlagBehaviour<,>));

            services.AddHttpClient("genderize", c =>
            {
                var baseUrl = configuration["ExternalApi:GenderizeUrl"]
                    ?? "https://api.genderize.io";

                c.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("agify", c =>
            {
                var baseUrl = configuration["ExternalApi:AgifyUrl"]
                    ?? "https://api.agify.io";
                c.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("nationalize", c =>
            {
                var baseUrl = configuration["ExternalApi:NationalizeUrl"]
                    ?? "https://api.nationalize.io";

                c.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
