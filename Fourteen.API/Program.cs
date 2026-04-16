using Fourteen.API.Extensions;
using Fourteen.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(8080);
//});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiMiddleware();

app.MapHealthChecks("/api/health");

app.Run();