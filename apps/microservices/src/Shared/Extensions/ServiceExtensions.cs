using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Shared.Middleware;

namespace Shared.Extensions;

/// <summary>
/// Extension methods for configuring common services across all microservices
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds common services including health checks, Swagger, CORS, and controllers
    /// </summary>
    public static IServiceCollection AddCommonServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        string serviceName,
        string serviceVersion = "v1")
    {
        // Add controllers
        services.AddControllers();

        // Add health checks
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self" });

        // Add Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(serviceVersion, new OpenApiInfo
            {
                Title = $"{serviceName} API",
                Version = serviceVersion,
                Description = $"API documentation for {serviceName}",
                Contact = new OpenApiContact
                {
                    Name = "Microservices Architecture",
                    Email = "support@microservices.local"
                }
            });

            // Include XML comments if available
            var xmlFile = $"{serviceName}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // Add HttpClient factory
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Configures common middleware pipeline
    /// </summary>
    public static IApplicationBuilder UseCommonMiddleware(
        this IApplicationBuilder app,
        string serviceName)
    {
        // Use Serilog request logging
        app.UseSerilogRequestLogging();

        // Use custom logging middleware
        app.UseMiddleware<LoggingMiddleware>();

        // Use custom metrics middleware
        app.UseMiddleware<MetricsMiddleware>(serviceName);

        // Use CORS
        app.UseCors("AllowAll");

        // Use routing
        app.UseRouting();

        // Use authorization
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Maps common endpoints including health checks, metrics, and controllers
    /// </summary>
    public static IApplicationBuilder MapCommonEndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            // Map controllers
            endpoints.MapControllers();

            // Map health checks
            endpoints.MapHealthChecks("/health");
            endpoints.MapHealthChecks("/health/ready");
            endpoints.MapHealthChecks("/health/live");

            // Map Prometheus metrics
            endpoints.MapMetrics();
        });

        return app;
    }

    /// <summary>
    /// Configures Serilog logging
    /// </summary>
    public static void ConfigureSerilog(string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate: 
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}

