using Microsoft.AspNetCore.Mvc;
using Prometheus;
using System.Diagnostics;

namespace HealthService.Controllers;

/// <summary>
/// Controller for health check ping operations
/// </summary>
[ApiController]
[Route("[controller]")]
public class PingController : ControllerBase
{
    private static readonly Counter PingCounter = Metrics.CreateCounter(
        "health_ping_requests_total",
        "Total number of ping requests");

    private static readonly Histogram PingDuration = Metrics.CreateHistogram(
        "health_ping_duration_seconds",
        "Duration of ping requests in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.LinearBuckets(0.001, 0.001, 10)
        });

    private readonly ILogger<PingController> _logger;

    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simple ping endpoint for testing minimal latency
    /// </summary>
    /// <returns>Returns "pong" string</returns>
    /// <response code="200">Returns pong response</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        using (PingDuration.NewTimer())
        {
            PingCounter.Inc();
            _logger.LogDebug("Ping request received");
            return Ok("pong");
        }
    }

    /// <summary>
    /// Ping endpoint with delay simulation
    /// </summary>
    /// <param name="delayMs">Delay in milliseconds (default: 0)</param>
    /// <returns>Returns "pong" string after delay</returns>
    /// <response code="200">Returns pong response</response>
    [HttpGet("delayed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PingWithDelay([FromQuery] int delayMs = 0)
    {
        using (PingDuration.NewTimer())
        {
            PingCounter.Inc();
            
            if (delayMs > 0)
            {
                _logger.LogInformation("Ping with {Delay}ms delay", delayMs);
                await Task.Delay(Math.Min(delayMs, 5000)); // Max 5 seconds
            }

            return Ok("pong");
        }
    }

    /// <summary>
    /// Returns ping statistics
    /// </summary>
    /// <returns>Ping statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStats()
    {
        return Ok(new
        {
            ServiceName = "HealthService",
            Status = "Running",
            Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            Timestamp = DateTime.UtcNow
        });
    }
}

