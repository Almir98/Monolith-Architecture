using Microsoft.AspNetCore.Mvc;

namespace MonolithApp.Controllers;

/// <summary>
/// CPU-intensive computation endpoints for performance testing
/// </summary>
[ApiController]
[Route("[controller]")]
public class ComputeController : ControllerBase
{
    private readonly ILogger<ComputeController> _logger;

    public ComputeController(ILogger<ComputeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// CPU-intensive Fibonacci calculation for performance testing
    /// </summary>
    /// <param name="n">Fibonacci number to calculate (default: 35)</param>
    /// <returns>Computation result with timing information</returns>
    /// <response code="200">Returns computation result</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Compute([FromQuery] int n = 35)
    {
        _logger.LogInformation("Starting CPU-intensive computation with n = {N}", n);
        
        var startTime = DateTime.UtcNow;
        var result = CalculateFibonacci(n);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        _logger.LogInformation("Computation completed in {Duration}ms", duration.TotalMilliseconds);

        return Ok(new
        {
            input = n,
            result = result,
            durationMs = duration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        });
    }

    private long CalculateFibonacci(int n)
    {
        if (n <= 1) return n;
        
        long a = 0, b = 1;
        for (int i = 2; i <= n; i++)
        {
            long temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }
}
