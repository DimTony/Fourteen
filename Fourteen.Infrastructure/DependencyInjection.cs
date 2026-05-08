using DnsClient;
using Fourteen.Application.Common.Behaviors;
using Fourteen.Application.Common.Helpers;
using Fourteen.Application.Interfaces;
using Fourteen.Infrastructure.Persistence;
using Fourteen.Infrastructure.Persistence.Repositories;
using Fourteen.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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
                .AddServices(configuration)
                .AddCaching();

            return services;
        }

        private static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {

          services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null);
    
                }),
                ServiceLifetime.Scoped);

            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            var redisString = configuration.GetConnectionString("RedisConnection");
            var disableRedis = configuration.GetValue<bool>("DisableRedis");

                options.AbortOnConnectFail = false;
                options.ConnectRetry = 3;

                services.AddSingleton<IConnectionMultiplexer>(
                    ConnectionMultiplexer.Connect(options)
                );
            }

            services.AddScoped<IRedisService, RedisServices>();

            return services;
        }

        private static IServiceCollection AddRepositories(
           this IServiceCollection services)
        {
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();    
            services.AddScoped<IDomainRepository, DomainRepository>();    
            services.AddScoped<IScanRepository, ScanRepository>();    
            services.AddScoped<IFindingRepository, FindingRepository>();    

            return services;
        }

        private static IServiceCollection AddCaching(
            this IServiceCollection services)
        {

            services.AddMemoryCache(opts =>
            {
                opts.SizeLimit = 10_000;
                opts.CompactionPercentage = 0.2;  
            });
    
            services.AddSingleton<IQueryCache, MemoryQueryCache>();
    
            return services;
        }


        private static IServiceCollection AddServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IServices, ExternalServices>();
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<IGithubClient, GithubService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<LookupClient>(_ =>
                new LookupClient(
                    new LookupClientOptions(
                        NameServer.GooglePublicDns,       // 8.8.8.8
                        NameServer.GooglePublicDns2       // 8.8.4.4
                    )
                    {
                        UseCache = false,                 // don't cache during testing
                        Retries  = 3,
                        Timeout  = TimeSpan.FromSeconds(5)
                    }
                )
            );
            services.AddScoped<IDnsService, DnsServices>();
            services.AddMemoryCache();
            services.AddScoped<NaturalLanguageQueryParser>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FeatureFlagBehaviour<,>));
            services.AddTransient<IBulkProfileImporter, BulkProfileImporter>();

            services.AddHttpClient("genderize", c =>
            {
                var baseUrl = configuration["ExternalApi:GenderizeUrl"]
                    ?? string.Empty;

                c.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("agify", c =>
            {
                var baseUrl = configuration["ExternalApi:AgifyUrl"]
                    ?? string.Empty;
                c.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("nationalize", c =>
            {
                var baseUrl = configuration["ExternalApi:NationalizeUrl"]
                    ?? string.Empty;

                c.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }