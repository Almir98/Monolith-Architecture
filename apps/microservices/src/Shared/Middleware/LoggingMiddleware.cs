using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Diagnostics;

namespace Shared.Middleware;

/// <summary>
/// Middleware for structured logging of HTTP requests and responses
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Log.ForContext<LoggingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID for request tracking
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        context.Response.Headers.Append("X-Correlation-ID", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            var sw = Stopwatch.StartNew();
            var request = context.Request;

            try
            {
                _logger.Information(
                    "HTTP {Method} {Path} started from {RemoteIp}",
                    request.Method,
                    request.Path,
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown");

                await _next(context);

                sw.Stop();
                var elapsedMs = sw.ElapsedMilliseconds;

                var logLevel = context.Response.StatusCode >= 500 
                    ? Serilog.Events.LogEventLevel.Error
                    : context.Response.StatusCode >= 400
                        ? Serilog.Events.LogEventLevel.Warning
                        : Serilog.Events.LogEventLevel.Information;

                _logger.Write(
                    logLevel,
                    "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    request.Method,
                    request.Path,
                    context.Response.StatusCode,
                    elapsedMs);

                // Log slow requests
                if (elapsedMs > 1000)
                {
                    _logger.Warning(
                        "SLOW REQUEST: HTTP {Method} {Path} took {Elapsed}ms",
                        request.Method,
                        request.Path,
                        elapsedMs);
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.Error(
                    ex,
                    "HTTP {Method} {Path} failed after {Elapsed}ms",
                    request.Method,
                    request.Path,
                    sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}

