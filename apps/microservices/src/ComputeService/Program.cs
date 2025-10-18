using ComputeService.Services;
using Serilog;
using Shared.Extensions;

// Configure Serilog
ServiceExtensions.ConfigureSerilog("ComputeService");

try
{
    Log.Information("Starting ComputeService...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add common services
    builder.Services.AddCommonServices(builder.Configuration, "ComputeService");

    // Add Compute service
    builder.Services.AddSingleton<IComputeService, ComputeService.Services.ComputeService>();

    var app = builder.Build();

    // Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerWithUI("ComputeService");
    }

    app.UseCommonMiddleware("ComputeService");
    app.MapCommonEndpoints();

    Log.Information("ComputeService started successfully on {Urls}",
        string.Join(", ", builder.Configuration.GetSection("Urls").Get<string[]>() ?? new[] { "http://localhost:5030" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ComputeService failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
