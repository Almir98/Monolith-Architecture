using Microsoft.AspNetCore.Mvc;
using BulkService.Models;
using BulkService.Services;
using Prometheus;

namespace BulkService.Controllers;

/// <summary>
/// Controller for bulk processing operations
/// </summary>
[ApiController]
[Route("[controller]")]
public class BulkController : ControllerBase
{
    private readonly IBulkService _bulkService;
    private readonly ILogger<BulkController> _logger;

    private static readonly Counter BulkJobsCounter = Metrics.CreateCounter(
        "bulk_jobs_total",
        "Total number of bulk jobs",
        new CounterConfiguration
        {
            LabelNames = new[] { "type" }
        });

    private static readonly Counter BulkItemsProcessedCounter = Metrics.CreateCounter(
        "bulk_items_processed_total",
        "Total number of bulk items processed");

    private static readonly Counter BulkItemsFailedCounter = Metrics.CreateCounter(
        "bulk_items_failed_total",
        "Total number of bulk items that failed");

    private static readonly Histogram BulkProcessingDuration = Metrics.CreateHistogram(
        "bulk_processing_duration_seconds",
        "Duration of bulk processing operations",
        new HistogramConfiguration
        {
            LabelNames = new[] { "type" },
            Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
        });

    private static readonly Gauge BulkActiveJobs = Metrics.CreateGauge(
        "bulk_active_jobs",
        "Number of active bulk jobs");

    public BulkController(IBulkService bulkService, ILogger<BulkController> logger)
    {
        _bulkService = bulkService;
        _logger = logger;
    }

    /// <summary>
    /// Processes items sequentially
    /// </summary>
    /// <param name="count">Number of items to process (max 1000)</param>
    /// <returns>Processing result</returns>
    /// <response code="200">Returns processing result</response>
    /// <response code="400">Invalid parameter</response>
    [HttpPost]
    [ProducesResponseType(typeof(BulkProcessingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkProcessingResult>> ProcessSequential([FromQuery] int count = 10)
    {
        if (count < 1 || count > 1000)
        {
            return BadRequest(new { error = "Count must be between 1 and 1000" });
        }

        using (BulkProcessingDuration.WithLabels("sequential").NewTimer())
        {
            BulkActiveJobs.Inc();
            try
            {
                _logger.LogInformation("Starting sequential bulk processing for {Count} items", count);
                
                var result = await _bulkService.ProcessSequentialAsync(count);

                BulkJobsCounter.WithLabels("sequential").Inc();
                BulkItemsProcessedCounter.Inc(result.SuccessCount);
                BulkItemsFailedCounter.Inc(result.FailedCount);

                return Ok(result);
            }
            finally
            {
                BulkActiveJobs.Dec();
            }
        }
    }

    /// <summary>
    /// Processes items in parallel
    /// </summary>
    /// <param name="count">Number of items to process (max 1000)</param>
    /// <param name="maxDegree">Maximum degree of parallelism (-1 for default)</param>
    /// <returns>Processing result</returns>
    /// <response code="200">Returns processing result</response>
    /// <response code="400">Invalid parameter</response>
    [HttpPost("parallel")]
    [ProducesResponseType(typeof(BulkProcessingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkProcessingResult>> ProcessParallel(
        [FromQuery] int count = 10,
        [FromQuery] int maxDegree = -1)
    {
        if (count < 1 || count > 1000)
        {
            return BadRequest(new { error = "Count must be between 1 and 1000" });
        }

        using (BulkProcessingDuration.WithLabels("parallel").NewTimer())
        {
            BulkActiveJobs.Inc();
            try
            {
                _logger.LogInformation("Starting parallel bulk processing for {Count} items", count);
                
                var result = await _bulkService.ProcessParallelAsync(count, maxDegree);

                BulkJobsCounter.WithLabels("parallel").Inc();
                BulkItemsProcessedCounter.Inc(result.SuccessCount);
                BulkItemsFailedCounter.Inc(result.FailedCount);

                return Ok(result);
            }
            finally
            {
                BulkActiveJobs.Dec();
            }
        }
    }

    /// <summary>
    /// Processes items in batches
    /// </summary>
    /// <param name="count">Number of items to process (max 1000)</param>
    /// <param name="batchSize">Batch size (default 50)</param>
    /// <returns>Processing result</returns>
    /// <response code="200">Returns processing result</response>
    /// <response code="400">Invalid parameter</response>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BulkProcessingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkProcessingResult>> ProcessBatch(
        [FromQuery] int count = 100,
        [FromQuery] int batchSize = 50)
    {
        if (count < 1 || count > 1000)
        {
            return BadRequest(new { error = "Count must be between 1 and 1000" });
        }

        if (batchSize < 1 || batchSize > 200)
        {
            return BadRequest(new { error = "Batch size must be between 1 and 200" });
        }

        using (BulkProcessingDuration.WithLabels("batch").NewTimer())
        {
            BulkActiveJobs.Inc();
            try
            {
                _logger.LogInformation("Starting batch bulk processing for {Count} items with batch size {BatchSize}",
                    count, batchSize);
                
                var result = await _bulkService.ProcessBatchAsync(count, batchSize);

                BulkJobsCounter.WithLabels("batch").Inc();
                BulkItemsProcessedCounter.Inc(result.SuccessCount);
                BulkItemsFailedCounter.Inc(result.FailedCount);

                return Ok(result);
            }
            finally
            {
                BulkActiveJobs.Dec();
            }
        }
    }

    /// <summary>
    /// Starts an async bulk job
    /// </summary>
    /// <param name="count">Number of items to process</param>
    /// <param name="type">Processing type (sequential, parallel, batch)</param>
    /// <returns>Job ID</returns>
    /// <response code="202">Job started</response>
    /// <response code="400">Invalid parameter</response>
    [HttpPost("async")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult StartAsyncJob([FromQuery] int count = 100, [FromQuery] string type = "sequential")
    {
        if (count < 1 || count > 5000)
        {
            return BadRequest(new { error = "Count must be between 1 and 5000" });
        }

        var validTypes = new[] { "sequential", "parallel", "batch" };
        if (!validTypes.Contains(type.ToLower()))
        {
            return BadRequest(new { error = $"Type must be one of: {string.Join(", ", validTypes)}" });
        }

        var jobId = _bulkService.StartAsyncJob(count, type);
        
        _logger.LogInformation("Started async job {JobId} for {Count} items with type {Type}",
            jobId, count, type);

        return AcceptedAtAction(nameof(GetJobStatus), new { jobId }, new { jobId, count, type });
    }

    /// <summary>
    /// Gets the status of a bulk job
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <returns>Job status</returns>
    /// <response code="200">Returns job status</response>
    /// <response code="404">Job not found</response>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(BulkJobStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BulkJobStatus> GetJobStatus(Guid jobId)
    {
        var status = _bulkService.GetJobStatus(jobId);
        
        if (status == null)
        {
            return NotFound(new { error = $"Job {jobId} not found" });
        }

        return Ok(status);
    }
}

