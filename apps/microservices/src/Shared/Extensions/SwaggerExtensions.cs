using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Extensions;

/// <summary>
/// Extension methods for Swagger configuration
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Configures Swagger UI with custom settings
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithUI(
        this IApplicationBuilder app,
        string serviceName,
        string version = "v1")
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{serviceName} {version}");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = $"{serviceName} API Documentation";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
        });

        return app;
    }
}

