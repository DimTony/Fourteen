using Fourteen.API.Extensions;
using Fourteen.Infrastructure;
using Fourteen.Infrastructure.Persistence;
using Fourteen.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for detailed logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Fourteen.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Log.Information("Starting Fourteen API...");
// Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

// // Log all configuration keys (without sensitive values)
// Log.Information("=== Configuration Diagnostics ===");
// Log.Information("Features:CreateProfile = {Value}", 
//     builder.Configuration.GetValue<bool>("Features:CreateProfile"));
// Log.Information("Features:GetProfileById = {Value}", 
//     builder.Configuration.GetValue<bool>("Features:GetProfileById"));
// Log.Information("Features:GetAllProfiles = {Value}", 
//     builder.Configuration.GetValue<bool>("Features:GetAllProfiles"));
// Log.Information("Features:DeleteProfile = {Value}", 
//     builder.Configuration.GetValue<bool>("Features:DeleteProfile"));
// Log.Information("Features:ClassifyName = {Value}", 
//     builder.Configuration.GetValue<bool>("Features:ClassifyName"));
// Log.Information("=================================");

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    var seeder = new ProfileSeeder(context);
    var jsonPath = Path.Combine(AppContext.BaseDirectory, "profiles.json");
    // await seeder.SeedAsync("Infrastructure/Seed/profiles.json");
    await seeder.SeedAsync(jsonPath);
}

// Log all registered routes on startup
// app.Lifetime.ApplicationStarted.Register(() =>
// {
//     var routes = app.Services.GetService<IEnumerable<EndpointDataSource>>();
//     if (routes != null)
//     {
//         Log.Information("=== Registered Endpoints ===");
//         foreach (var endpointDataSource in routes)
//         {
//             foreach (var endpoint in endpointDataSource.Endpoints)
//             {
//                 if (endpoint is RouteEndpoint routeEndpoint)
//                 {
//                     Log.Information("Route: {Pattern} | DisplayName: {DisplayName}", 
//                         routeEndpoint.RoutePattern.RawText, 
//                         routeEndpoint.DisplayName);
//                 }
//             }
//         }
//         Log.Information("============================");
//     }
// });

app.UseApiMiddleware();

// Add request logging middleware
// app.Use(async (context, next) =>
// {
//     var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//     logger.LogInformation("Incoming Request: {Method} {Path} from {RemoteIp}", 
//         context.Request.Method, 
//         context.Request.Path,
//         context.Connection.RemoteIpAddress);
    
//     await next();
    
//     logger.LogInformation("Response: {StatusCode} for {Method} {Path}", 
//         context.Response.StatusCode,
//         context.Request.Method, 
//         context.Request.Path);
// });

app.MapHealthChecks("/api/health");

try
{
    Log.Information("Application configured successfully. Starting web host...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}