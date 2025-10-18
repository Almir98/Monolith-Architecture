using BulkService.Models;

namespace BulkService.Services;

/// <summary>
/// Interface for bulk processing operations
/// </summary>
public interface IBulkService
{
    /// <summary>
    /// Processes items sequentially
    /// </summary>
    Task<BulkProcessingResult> ProcessSequentialAsync(int count);

    /// <summary>
    /// Processes items in parallel
    /// </summary>
    Task<BulkProcessingResult> ProcessParallelAsync(int count, int maxDegreeOfParallelism = -1);

    /// <summary>
    /// Processes items in batches
    /// </summary>
    Task<BulkProcessingResult> ProcessBatchAsync(int count, int batchSize);

    /// <summary>
    /// Gets job status
    /// </summary>
    BulkJobStatus? GetJobStatus(Guid jobId);

    /// <summary>
    /// Starts an async bulk job
    /// </summary>
    Guid StartAsyncJob(int count, string processingType);
}

