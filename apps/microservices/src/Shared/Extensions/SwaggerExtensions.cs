using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Extensions;

/// <summary>
/// Extension methods for Swagger configuration
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Configures Swagger UI with custom settings and root redirect
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithUI(
        this IApplicationBuilder app,
        string serviceName,
        string version = "v1")
    {
        // Add redirect from root to Swagger
        app.UseSwaggerRootRedirect();
        
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

    /// <summary>
    /// Adds middleware to redirect from root (/) to Swagger UI (/swagger)
    /// </summary>
    public static IApplicationBuilder UseSwaggerRootRedirect(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger", permanent: false);
                return;
            }
            await next();
        });

        return app;
    }
}

