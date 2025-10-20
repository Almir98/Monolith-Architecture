using Serilog;
using Shared.Extensions;
using Prometheus;

// Configure Serilog
ServiceExtensions.ConfigureSerilog("ApiGateway");

try
{
    Log.Information("Starting ApiGateway...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // Add health checks
    builder.Services.AddHealthChecks();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    // Add YARP reverse proxy
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // Add Swagger for Gateway
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "API Gateway",
            Version = "v1",
            Description = "Central API Gateway for all microservices"
        });
    });

    var app = builder.Build();

    // Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        // Add redirect from root to Swagger
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger", permanent: false);
                return;
            }
            await next();
        });
        
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
            options.RoutePrefix = "swagger";
        });
    }

    // Use Serilog request logging
    app.UseSerilogRequestLogging();

    // Use custom logging middleware
    app.UseMiddleware<Shared.Middleware.LoggingMiddleware>();

    // Use custom metrics middleware
    app.UseMiddleware<Shared.Middleware.MetricsMiddleware>("ApiGateway");

    app.UseCors("AllowAll");
    app.UseRouting();

    // Map endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");
    app.MapMetrics();

    // Map reverse proxy
    app.MapReverseProxy();

    // Add info endpoint (instead of root)
    app.MapGet("/info", () => new
    {
        service = "API Gateway",
        version = "1.0",
        status = "running",
        timestamp = DateTime.UtcNow,
        routes = new[]
        {
            "/api/health - HealthService endpoints",
            "/api/ping - Ping endpoints",
            "/api/orders - OrderService endpoints",
            "/api/compute - ComputeService endpoints",
            "/api/bulk - BulkService endpoints",
            "/health - Gateway health check",
            "/metrics - Prometheus metrics",
            "/swagger - API documentation"
        }
    });

    Log.Information("ApiGateway started successfully on {Urls}",
        string.Join(", ", builder.Configuration.GetSection("Urls").Get<string[]>() ?? new[] { "http://localhost:80" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ApiGateway failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
