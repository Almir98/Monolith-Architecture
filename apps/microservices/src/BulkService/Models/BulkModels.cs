namespace BulkService.Models;

/// <summary>
/// Represents a single bulk item
/// </summary>
public class BulkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Data { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the status of a bulk job
/// </summary>
public class BulkJobStatus
{
    public Guid JobId { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public string Status { get; set; } = "Running"; // Running, Completed, Failed
    public string ProcessingType { get; set; } = "Sequential"; // Sequential, Parallel, Batch
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt?.Subtract(StartedAt);
    public double? ItemsPerSecond => Duration?.TotalSeconds > 0 
        ? ProcessedItems / Duration.Value.TotalSeconds 
        : null;
}

/// <summary>
/// Result of bulk processing operation
/// </summary>
public class BulkProcessingResult
{
    public Guid JobId { get; set; }
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkItem> Items { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public double ItemsPerSecond => ProcessingTime.TotalSeconds > 0 
        ? TotalCount / ProcessingTime.TotalSeconds 
        : 0;
}

