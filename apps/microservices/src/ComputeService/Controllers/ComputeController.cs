using Microsoft.AspNetCore.Mvc;
using ComputeService.Services;
using Prometheus;
using System.Diagnostics;

namespace ComputeService.Controllers;

/// <summary>
/// Controller for CPU-intensive compute operations
/// </summary>
[ApiController]
[Route("[controller]")]
public class ComputeController : ControllerBase
{
    private readonly IComputeService _computeService;
    private readonly ILogger<ComputeController> _logger;
    private readonly IConfiguration _configuration;

    private static readonly Counter ComputeRequestsCounter = Metrics.CreateCounter(
        "compute_requests_total",
        "Total number of compute requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "operation" }
        });

    private static readonly Histogram ComputeDuration = Metrics.CreateHistogram(
        "compute_duration_seconds",
        "Duration of compute operations in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "operation" },
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
        });

    private static readonly Histogram ComputeComplexity = Metrics.CreateHistogram(
        "compute_complexity",
        "Complexity parameter for compute operations",
        new HistogramConfiguration
        {
            LabelNames = new[] { "operation" },
            Buckets = Histogram.LinearBuckets(10, 10, 10)
        });

    public ComputeController(
        IComputeService computeService,
        ILogger<ComputeController> logger,
        IConfiguration configuration)
    {
        _computeService = computeService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Calculates Fibonacci number
    /// </summary>
    /// <param name="n">Fibonacci sequence index (max 50)</param>
    /// <returns>Fibonacci result</returns>
    /// <response code="200">Returns Fibonacci result</response>
    /// <response code="400">Invalid parameter</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CalculateFibonacci([FromQuery] int n = 10)
    {
        var maxFibonacci = _configuration.GetValue<int>("ComputeLimits:MaxFibonacci", 50);
        
        if (n < 0 || n > maxFibonacci)
        {
            return BadRequest(new { error = $"Parameter n must be between 0 and {maxFibonacci}" });
        }

        using (ComputeDuration.WithLabels("fibonacci").NewTimer())
        {
            var sw = Stopwatch.StartNew();
            var result = _computeService.CalculateFibonacci(n);
            sw.Stop();

            ComputeRequestsCounter.WithLabels("fibonacci").Inc();
            ComputeComplexity.WithLabels("fibonacci").Observe(n);

            _logger.LogInformation("Fibonacci({N}) = {Result}, took {ElapsedMs}ms", n, result, sw.ElapsedMilliseconds);

            return Ok(new
            {
                operation = "fibonacci",
                input = n,
                result = result,
                computeTimeMs = sw.ElapsedMilliseconds
            });
        }
    }

    /// <summary>
    /// Approximates Pi using Monte Carlo method
    /// </summary>
    /// <param name="iterations">Number of iterations (max 1000000)</param>
    /// <returns>Pi approximation result</returns>
    /// <response code="200">Returns Pi approximation</response>
    /// <response code="400">Invalid parameter</response>
    [HttpGet("pi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ApproximatePi([FromQuery] int iterations = 10000)
    {
        var maxIterations = _configuration.GetValue<int>("ComputeLimits:MaxPiIterations", 1000000);
        
        if (iterations < 1 || iterations > maxIterations)
        {
            return BadRequest(new { error = $"Iterations must be between 1 and {maxIterations}" });
        }

        using (ComputeDuration.WithLabels("pi").NewTimer())
        {
            var sw = Stopwatch.StartNew();
            var result = _computeService.ApproximatePi(iterations);
            sw.Stop();

            ComputeRequestsCounter.WithLabels("pi").Inc();
            ComputeComplexity.WithLabels("pi").Observe(iterations);

            _logger.LogInformation("Pi approximation with {Iterations} iterations = {Result}, took {ElapsedMs}ms",
                iterations, result, sw.ElapsedMilliseconds);

            return Ok(new
            {
                operation = "pi_approximation",
                iterations = iterations,
                result = result,
                actualPi = Math.PI,
                error = Math.Abs(result - Math.PI),
                computeTimeMs = sw.ElapsedMilliseconds
            });
        }
    }

    /// <summary>
    /// Finds all prime numbers up to a limit
    /// </summary>
    /// <param name="limit">Upper limit for prime search (max 1000000)</param>
    /// <returns>List of prime numbers</returns>
    /// <response code="200">Returns list of primes</response>
    /// <response code="400">Invalid parameter</response>
    [HttpGet("prime")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult FindPrimes([FromQuery] int limit = 100)
    {
        var maxLimit = _configuration.GetValue<int>("ComputeLimits:MaxPrimeLimit", 1000000);
        
        if (limit < 1 || limit > maxLimit)
        {
            return BadRequest(new { error = $"Limit must be between 1 and {maxLimit}" });
        }

        using (ComputeDuration.WithLabels("prime").NewTimer())
        {
            var sw = Stopwatch.StartNew();
            var primes = _computeService.FindPrimes(limit);
            sw.Stop();

            ComputeRequestsCounter.WithLabels("prime").Inc();
            ComputeComplexity.WithLabels("prime").Observe(limit);

            _logger.LogInformation("Found {Count} primes up to {Limit}, took {ElapsedMs}ms",
                primes.Count, limit, sw.ElapsedMilliseconds);

            return Ok(new
            {
                operation = "prime_numbers",
                limit = limit,
                count = primes.Count,
                primes = limit <= 1000 ? primes : null, // Only return list if small
                computeTimeMs = sw.ElapsedMilliseconds
            });
        }
    }

    /// <summary>
    /// Performs CPU benchmark
    /// </summary>
    /// <param name="iterations">Number of iterations (max 10000000)</param>
    /// <returns>Benchmark result</returns>
    /// <response code="200">Returns benchmark result</response>
    /// <response code="400">Invalid parameter</response>
    [HttpGet("benchmark")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Benchmark([FromQuery] int iterations = 100000)
    {
        if (iterations < 1 || iterations > 10000000)
        {
            return BadRequest(new { error = "Iterations must be between 1 and 10000000" });
        }

        using (ComputeDuration.WithLabels("benchmark").NewTimer())
        {
            var sw = Stopwatch.StartNew();
            var result = _computeService.PerformBenchmark(iterations);
            sw.Stop();

            ComputeRequestsCounter.WithLabels("benchmark").Inc();
            ComputeComplexity.WithLabels("benchmark").Observe(iterations);

            _logger.LogInformation("Benchmark with {Iterations} iterations completed in {ElapsedMs}ms",
                iterations, sw.ElapsedMilliseconds);

            return Ok(new
            {
                operation = "benchmark",
                iterations = iterations,
                result = result,
                computeTimeMs = sw.ElapsedMilliseconds
            });
        }
    }
}

