using BulkService.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace BulkService.Services;

/// <summary>
/// Implementation of bulk processing operations
/// </summary>
public class BulkService : IBulkService
{
    private readonly ILogger<BulkService> _logger;
    private readonly ConcurrentDictionary<Guid, BulkJobStatus> _jobs = new();

    public BulkService(ILogger<BulkService> logger)
    {
        _logger = logger;
    }

    public async Task<BulkProcessingResult> ProcessSequentialAsync(int count)
    {
        var jobId = Guid.NewGuid();
        var sw = Stopwatch.StartNew();
        var items = new List<BulkItem>();

        _logger.LogInformation("Starting sequential processing of {Count} items", count);

        for (int i = 0; i < count; i++)
        {
            var item = await ProcessSingleItemAsync(i);
            items.Add(item);
        }

        sw.Stop();

        _logger.LogInformation("Sequential processing completed: {Count} items in {ElapsedMs}ms",
            count, sw.ElapsedMilliseconds);

        return new BulkProcessingResult
        {
            JobId = jobId,
            TotalCount = count,
            SuccessCount = items.Count(x => x.Success),
            FailedCount = items.Count(x => !x.Success),
            Items = items,
            ProcessingTime = sw.Elapsed,
            ProcessingType = "Sequential"
        };
    }

    public async Task<BulkProcessingResult> ProcessParallelAsync(int count, int maxDegreeOfParallelism = -1)
    {
        var jobId = Guid.NewGuid();
        var sw = Stopwatch.StartNew();
        var items = new ConcurrentBag<BulkItem>();

        _logger.LogInformation("Starting parallel processing of {Count} items with max degree {MaxDegree}",
            count, maxDegreeOfParallelism == -1 ? "unlimited" : maxDegreeOfParallelism);

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism == -1 
                ? Environment.ProcessorCount 
                : maxDegreeOfParallelism
        };

        await Parallel.ForEachAsync(
            Enumerable.Range(0, count),
            options,
            async (i, ct) =>
            {
                var item = await ProcessSingleItemAsync(i);
                items.Add(item);
            });

        sw.Stop();

        var itemsList = items.ToList();

        _logger.LogInformation("Parallel processing completed: {Count} items in {ElapsedMs}ms",
            count, sw.ElapsedMilliseconds);

        return new BulkProcessingResult
        {
            JobId = jobId,
            TotalCount = count,
            SuccessCount = itemsList.Count(x => x.Success),
            FailedCount = itemsList.Count(x => !x.Success),
            Items = itemsList,
            ProcessingTime = sw.Elapsed,
            ProcessingType = "Parallel"
        };
    }

    public async Task<BulkProcessingResult> ProcessBatchAsync(int count, int batchSize)
    {
        var jobId = Guid.NewGuid();
        var sw = Stopwatch.StartNew();
        var items = new List<BulkItem>();

        _logger.LogInformation("Starting batch processing of {Count} items with batch size {BatchSize}",
            count, batchSize);

        for (int i = 0; i < count; i += batchSize)
        {
            var currentBatchSize = Math.Min(batchSize, count - i);
            var batchTasks = Enumerable.Range(i, currentBatchSize)
                .Select(index => ProcessSingleItemAsync(index))
                .ToList();

            var batchResults = await Task.WhenAll(batchTasks);
            items.AddRange(batchResults);

            _logger.LogDebug("Processed batch {BatchNumber}: items {Start}-{End}",
                i / batchSize + 1, i, i + currentBatchSize - 1);
        }

        sw.Stop();

        _logger.LogInformation("Batch processing completed: {Count} items in {ElapsedMs}ms",
            count, sw.ElapsedMilliseconds);

        return new BulkProcessingResult
        {
            JobId = jobId,
            TotalCount = count,
            SuccessCount = items.Count(x => x.Success),
            FailedCount = items.Count(x => !x.Success),
            Items = items,
            ProcessingTime = sw.Elapsed,
            ProcessingType = $"Batch (size: {batchSize})"
        };
    }

    public BulkJobStatus? GetJobStatus(Guid jobId)
    {
        _jobs.TryGetValue(jobId, out var status);
        return status;
    }

    public Guid StartAsyncJob(int count, string processingType)
    {
        var jobId = Guid.NewGuid();
        var status = new BulkJobStatus
        {
            JobId = jobId,
            TotalItems = count,
            ProcessingType = processingType,
            StartedAt = DateTime.UtcNow,
            Status = "Running"
        };

        _jobs[jobId] = status;

        // Start background job
        _ = Task.Run(async () =>
        {
            try
            {
                BulkProcessingResult result = processingType.ToLower() switch
                {
                    "parallel" => await ProcessParallelAsync(count),
                    "batch" => await ProcessBatchAsync(count, 50),
                    _ => await ProcessSequentialAsync(count)
                };

                status.ProcessedItems = result.TotalCount;
                status.SuccessfulItems = result.SuccessCount;
                status.FailedItems = result.FailedCount;
                status.Status = "Completed";
                status.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Async job {JobId} failed", jobId);
                status.Status = "Failed";
                status.CompletedAt = DateTime.UtcNow;
            }
        });

        return jobId;
    }

    private async Task<BulkItem> ProcessSingleItemAsync(int index)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            // Simulate some processing work
            await Task.Delay(Random.Shared.Next(1, 10));
            
            // Simulate 5% failure rate
            var success = Random.Shared.NextDouble() > 0.05;

            sw.Stop();

            return new BulkItem
            {
                Data = $"Item-{index}",
                ProcessedAt = DateTime.UtcNow,
                ProcessingTime = sw.Elapsed,
                Success = success,
                ErrorMessage = success ? null : "Simulated processing error"
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new BulkItem
            {
                Data = $"Item-{index}",
                ProcessedAt = DateTime.UtcNow,
                ProcessingTime = sw.Elapsed,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

