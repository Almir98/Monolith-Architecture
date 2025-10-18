using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;
using Serilog;
using Shared.Extensions;

// Configure Serilog
ServiceExtensions.ConfigureSerilog("OrderService");

try
{
    Log.Information("Starting OrderService...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add common services
    builder.Services.AddCommonServices(builder.Configuration, "OrderService");

    // Add DbContext
    builder.Services.AddDbContext<OrderDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=orders.db";
        options.UseSqlite(connectionString);
    });

    // Add Order service
    builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();

    var app = builder.Build();

    // Initialize database
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        try
        {
            Log.Information("Ensuring database is created and migrated...");
            dbContext.Database.EnsureCreated();
            Log.Information("Database initialization completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing database");
            throw;
        }
    }

    // Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerWithUI("OrderService");
    }

    app.UseCommonMiddleware("OrderService");
    app.MapCommonEndpoints();

    Log.Information("OrderService started successfully on {Urls}",
        string.Join(", ", builder.Configuration.GetSection("Urls").Get<string[]>() ?? new[] { "http://localhost:5020" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrderService failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
