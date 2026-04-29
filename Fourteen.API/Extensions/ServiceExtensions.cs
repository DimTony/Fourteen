using FluentValidation;
using Fourteen.API.Middleware;
using Fourteen.Application;
using Fourteen.Application.Common.Behaviors;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Net;
using Fourteen.Application.Common.Config;

namespace Fourteen.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                if (ips.Length > 0 && !string.IsNullOrEmpty(ips[0]))
                {
                    return ips[0].Trim();
                }
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp.Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        public static IServiceCollection AddApiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            var rateLimitConfig = configuration
                .GetSection("RateLimiting")
                .Get<RateLimitingOptions>() ?? new RateLimitingOptions();
                
            services.AddRateLimiter(options =>
            {
                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetClientIpAddress(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitConfig.Auth.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitConfig.Auth.WindowSeconds)
                        }
                    )
                );
         
                options.AddPolicy("api", httpContext =>
                {
                    string partitionKey;

                    var authHeader = httpContext.Request.Headers["Authorization"]
                                        .FirstOrDefault();
                    var token = authHeader?.StartsWith("Bearer ") == true
                        ? authHeader["Bearer ".Length..]
                        : httpContext.Request.Cookies["access_token"];

                    if (!string.IsNullOrEmpty(token))
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        if (handler.CanReadToken(token))
                        {
                            var jwt = handler.ReadJwtToken(token);
                            partitionKey = jwt.Claims
                                .FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)
                                ?.Value ?? GetClientIpAddress(httpContext);
                        }
                        else
                        {
                            partitionKey = GetClientIpAddress(httpContext);
                        }
                    }
                    else
                    {
                        partitionKey = GetClientIpAddress(httpContext);
                    }

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitConfig.Api.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitConfig.Api.WindowSeconds)
                        }
                    );
                });

                options.OnRejected = async (ctx, _) =>
                {
                    ctx.HttpContext.Response.StatusCode = 429;
                    await ctx.HttpContext.Response.WriteAsJsonAsync(new { 
                        status = "error", message = "Too many requests" });
                };
            });

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

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT access token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                options.OperationFilter<ApiVersionHeaderFilter>();

            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var allowedOrigins = configuration.GetValue<string>("App:AllowedOrigins")?.Split(",") 
                        ?? new[] { "http://localhost:3000" };
                    
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            var jwtSecret = configuration.GetValue<string>("Jwt:SecretKey");
            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("Jwt:SecretKey is not configured in appsettings");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.ContainsKey("access_token"))
                        {
                            context.Token = context.Request.Cookies["access_token"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("admin"));
                options.AddPolicy("AnyRole",   p => p.RequireRole("admin", "analyst"));
            });

            services.AddHealthChecks();

            return services;
        }
    }
}
