using Serilog;
using Shared.Extensions;

// Configure Serilog
ServiceExtensions.ConfigureSerilog("HealthService");

try
{
    Log.Information("Starting HealthService...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add common services
    builder.Services.AddCommonServices(builder.Configuration, "HealthService");

    var app = builder.Build();

    // Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerWithUI("HealthService");
    }

    app.UseCommonMiddleware("HealthService");
    app.MapCommonEndpoints();

    Log.Information("HealthService started successfully on {Urls}", 
        string.Join(", ", builder.Configuration.GetSection("Urls").Get<string[]>() ?? new[] { "http://localhost:5010" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "HealthService failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
