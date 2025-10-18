namespace ComputeService.Services;

/// <summary>
/// Implementation of compute-intensive operations
/// </summary>
public class ComputeService : IComputeService
{
    private readonly ILogger<ComputeService> _logger;

    public ComputeService(ILogger<ComputeService> logger)
    {
        _logger = logger;
    }

    public long CalculateFibonacci(int n)
    {
        _logger.LogDebug("Calculating Fibonacci for n={N}", n);

        if (n <= 1) return n;
        if (n == 2) return 1;

        long prev = 0, current = 1;
        
        for (int i = 2; i <= n; i++)
        {
            long next = prev + current;
            prev = current;
            current = next;
        }

        return current;
    }

    public double ApproximatePi(int iterations)
    {
        _logger.LogDebug("Approximating Pi with {Iterations} iterations", iterations);

        int insideCircle = 0;
        var random = new Random();

        for (int i = 0; i < iterations; i++)
        {
            double x = random.NextDouble();
            double y = random.NextDouble();

            if (x * x + y * y <= 1)
            {
                insideCircle++;
            }
        }

        return 4.0 * insideCircle / iterations;
    }

    public List<int> FindPrimes(int limit)
    {
        _logger.LogDebug("Finding primes up to {Limit}", limit);

        if (limit < 2) return new List<int>();

        // Sieve of Eratosthenes
        bool[] isPrime = new bool[limit + 1];
        for (int i = 2; i <= limit; i++)
        {
            isPrime[i] = true;
        }

        for (int i = 2; i * i <= limit; i++)
        {
            if (isPrime[i])
            {
                for (int j = i * i; j <= limit; j += i)
                {
                    isPrime[j] = false;
                }
            }
        }

        var primes = new List<int>();
        for (int i = 2; i <= limit; i++)
        {
            if (isPrime[i])
            {
                primes.Add(i);
            }
        }

        return primes;
    }

    public double PerformBenchmark(int iterations)
    {
        _logger.LogDebug("Performing benchmark with {Iterations} iterations", iterations);

        double result = 0;
        for (int i = 0; i < iterations; i++)
        {
            result += Math.Sqrt(i) * Math.Sin(i) * Math.Cos(i);
        }

        return result;
    }
}

