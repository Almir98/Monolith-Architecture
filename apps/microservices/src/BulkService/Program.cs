using BulkService.Services;
using Serilog;
using Shared.Extensions;

// Configure Serilog
ServiceExtensions.ConfigureSerilog("BulkService");

try
{
    Log.Information("Starting BulkService...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add common services
    builder.Services.AddCommonServices(builder.Configuration, "BulkService");

    // Add Bulk service
    builder.Services.AddSingleton<IBulkService, BulkService.Services.BulkService>();

    var app = builder.Build();

    // Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerWithUI("BulkService");
    }

    app.UseCommonMiddleware("BulkService");
    app.MapCommonEndpoints();

    Log.Information("BulkService started successfully on {Urls}",
        string.Join(", ", builder.Configuration.GetSection("Urls").Get<string[]>() ?? new[] { "http://localhost:5040" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BulkService failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
