namespace ComputeService.Services;

/// <summary>
/// Interface for compute-intensive operations
/// </summary>
public interface IComputeService
{
    /// <summary>
    /// Calculates Fibonacci number
    /// </summary>
    long CalculateFibonacci(int n);

    /// <summary>
    /// Approximates Pi using Monte Carlo method
    /// </summary>
    double ApproximatePi(int iterations);

    /// <summary>
    /// Finds all prime numbers up to a limit
    /// </summary>
    List<int> FindPrimes(int limit);

    /// <summary>
    /// Performs a CPU-intensive computation for benchmarking
    /// </summary>
    double PerformBenchmark(int iterations);
}

