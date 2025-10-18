using Microsoft.AspNetCore.Http;
using Prometheus;
using System.Diagnostics;

namespace Shared.Middleware;

/// <summary>
/// Middleware for tracking HTTP request metrics with Prometheus
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _serviceName;

    private static readonly Counter RequestCounter = Metrics.CreateCounter(
        "http_requests_total",
        "Total number of HTTP requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "service", "method", "endpoint", "status" }
        });

    private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
        "http_request_duration_seconds",
        "Duration of HTTP requests in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "service", "method", "endpoint" },
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        });

    private static readonly Gauge RequestsInProgress = Metrics.CreateGauge(
        "http_requests_in_progress",
        "Number of HTTP requests currently in progress",
        new GaugeConfiguration
        {
            LabelNames = new[] { "service" }
        });

    private static readonly Histogram ResponseSize = Metrics.CreateHistogram(
        "http_response_size_bytes",
        "Size of HTTP responses in bytes",
        new HistogramConfiguration
        {
            LabelNames = new[] { "service", "endpoint" },
            Buckets = Histogram.ExponentialBuckets(100, 2, 10)
        });

    public MetricsMiddleware(RequestDelegate next, string serviceName)
    {
        _next = next;
        _serviceName = serviceName;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip metrics endpoint itself
        if (context.Request.Path.StartsWithSegments("/metrics"))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        // Normalize endpoint path (remove IDs and GUIDs)
        var normalizedPath = NormalizePath(path);

        using (RequestsInProgress.WithLabels(_serviceName).TrackInProgress())
        {
            var sw = Stopwatch.StartNew();
            
            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;
            
            try
            {
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                sw.Stop();

                var statusCode = context.Response.StatusCode.ToString();
                var responseSize = memoryStream.Length;

                // Record metrics
                RequestCounter.WithLabels(_serviceName, method, normalizedPath, statusCode).Inc();
                RequestDuration.WithLabels(_serviceName, method, normalizedPath).Observe(sw.Elapsed.TotalSeconds);
                ResponseSize.WithLabels(_serviceName, normalizedPath).Observe(responseSize);

                // Copy the response back to the original stream
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception)
            {
                sw.Stop();
                RequestCounter.WithLabels(_serviceName, method, normalizedPath, "500").Inc();
                RequestDuration.WithLabels(_serviceName, method, normalizedPath).Observe(sw.Elapsed.TotalSeconds);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "/";

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var normalizedSegments = new List<string>();

        foreach (var segment in segments)
        {
            // Replace numeric IDs with {id}
            if (int.TryParse(segment, out _))
            {
                normalizedSegments.Add("{id}");
            }
            // Replace GUIDs with {guid}
            else if (Guid.TryParse(segment, out _))
            {
                normalizedSegments.Add("{guid}");
            }
            else
            {
                normalizedSegments.Add(segment);
            }
        }

        return "/" + string.Join("/", normalizedSegments);
    }
}

