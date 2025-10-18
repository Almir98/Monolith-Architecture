using Microsoft.AspNetCore.Mvc;

namespace MonolithApp.Controllers;

/// <summary>
/// Health check endpoints for monitoring and load testing
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for latency measurement
    /// </summary>
    /// <returns>Returns "pong" for health check</returns>
    /// <response code="200">Returns "pong"</response>
    [HttpGet("ping")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        _logger.LogInformation("Health check ping requested");
        return Ok("pong");
    }
}
