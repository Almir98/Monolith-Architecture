using Microsoft.EntityFrameworkCore;
using MonolithApp.Data;
using MonolithApp.Services;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<IOrderService, OrderService>();

// Add health checks
builder.Services.AddHealthChecks();

// Add Prometheus metrics
// Note: Prometheus metrics are handled by the Prometheus NuGet package
// and don't require explicit registration in the service collection

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MonolithApp API",
        Version = "v1",
        Description = "A .NET 8 Web API monolithic application with monitoring and load testing capabilities",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "MonolithApp",
            Email = "contact@monolithapp.com"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for easy API testing

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
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MonolithApp API v1");
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger instead of /swagger/index.html
    c.DocumentTitle = "MonolithApp API Documentation";
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
});

// Add Prometheus metrics middleware
app.UseHttpMetrics();

// Add custom metrics middleware
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var endpoint = context.Request.Path.Value ?? "/";
    
    // Increment request counter
    Metrics.CreateCounter("http_requests_total", "Total HTTP requests", new[] { "method", "endpoint" })
        .WithLabels(method, endpoint).Inc();
    
    var startTime = DateTime.UtcNow;
    await next();
    var duration = DateTime.UtcNow - startTime;
    
    // Record request duration
    Metrics.CreateHistogram("http_request_duration_seconds", "HTTP request duration", new[] { "method", "endpoint" })
        .WithLabels(method, endpoint).Observe(duration.TotalSeconds);
});

// Skip HTTPS redirection in Docker containers
if (!app.Environment.IsDevelopment() || !app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Add Prometheus metrics endpoint
app.MapMetrics("/metrics");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

try
{
    Log.Information("Starting MonolithApp");
    
    // Start the application in background
    var appTask = Task.Run(() => app.Run());
    
    // Wait for the app to start
    await Task.Delay(3000);
    
    // Try to detect the actual port and open Swagger UI
    var ports = new[] { 5000, 5001, 62552, 7000, 7001, 8000, 8001 };
    var swaggerOpened = false;
    
    foreach (var port in ports)
    {
        try
        {
            var testUrl = $"http://localhost:{port}/health/ping";
            var response = await new HttpClient().GetAsync(testUrl);
            if (response.IsSuccessStatusCode)
            {
                var swaggerUrl = $"http://localhost:{port}/swagger";
                Log.Information("🌐 Found application on port {Port}, opening Swagger UI at: {SwaggerUrl}", port, swaggerUrl);
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = swaggerUrl,
                    UseShellExecute = true
                });
                swaggerOpened = true;
                break;
            }
        }
        catch
        {
            // Continue to next port
        }
    }
    
    if (!swaggerOpened)
    {
        Log.Warning("Could not auto-open Swagger UI. Please check the console output for the correct URL and add /swagger to it.");
    }
    
    // Wait for the app to complete
    await appTask;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
