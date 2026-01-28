using Polly;
using Polly.CircuitBreaker;

namespace CAP.BSP.DSP.Infrastructure.Resilience;

/// <summary>
/// Circuit breaker policies for MongoDB validation operations.
/// Prevents cascading failures when MongoDB reference data validation fails.
/// </summary>
public static class CircuitBreakerPolicies
{
    /// <summary>
    /// Circuit breaker for MongoDB validation operations.
    /// Opens after 5 consecutive failures, allows retry after 30 seconds.
    /// </summary>
    public static readonly ResiliencePipeline MongoDbValidationCircuitBreaker = new ResiliencePipelineBuilder()
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30),
            OnOpened = args =>
            {
                Console.WriteLine($"Circuit breaker opened due to: {args.Outcome.Exception?.Message}");
                return default;
            },
            OnClosed = args =>
            {
                Console.WriteLine("Circuit breaker closed - service recovered");
                return default;
            },
            OnHalfOpened = args =>
            {
                Console.WriteLine("Circuit breaker half-opened - testing service");
                return default;
            }
        })
        .Build();

    /// <summary>
    /// Retry policy for transient MongoDB failures.
    /// Retries 3 times with exponential backoff (2s, 4s, 8s).
    /// </summary>
    public static readonly ResiliencePipeline MongoDbRetryPolicy = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = Polly.DelayBackoffType.Exponential,
            OnRetry = args =>
            {
                Console.WriteLine($"Retrying MongoDB operation (attempt {args.AttemptNumber}): {args.Outcome.Exception?.Message}");
                return default;
            }
        })
        .Build();
}
