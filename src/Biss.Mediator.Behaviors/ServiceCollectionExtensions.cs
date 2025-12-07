using Biss.Mediator.Abstractions;
using Biss.Mediator.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Extension methods for registering pipeline behaviors.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds logging behavior to the pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorLoggingBehavior(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }

    /// <summary>
    /// Adds performance monitoring behavior to the pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="slowRequestThreshold">Threshold for considering a request slow.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorPerformanceBehavior(
        this IServiceCollection services, 
        TimeSpan? slowRequestThreshold = null)
    {
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        
        if (slowRequestThreshold.HasValue)
        {
            services.Configure<PerformanceBehaviorOptions>(options =>
            {
                options.SlowRequestThreshold = slowRequestThreshold.Value;
            });
        }
        
        return services;
    }

    /// <summary>
    /// Adds caching behavior to the pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="defaultCacheDuration">Default cache duration for query results.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorCachingBehavior(
        this IServiceCollection services,
        TimeSpan? defaultCacheDuration = null)
    {
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        
        if (defaultCacheDuration.HasValue)
        {
            services.Configure<CachingBehaviorOptions>(options =>
            {
                options.DefaultCacheDuration = defaultCacheDuration.Value;
            });
        }
        
        return services;
    }

    /// <summary>
    /// Adds validation behavior to the pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorValidationBehavior(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }

    /// <summary>
    /// Adds retry behavior to the pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts.</param>
    /// <param name="delayBetweenRetries">Delay between retry attempts.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorRetryBehavior(
        this IServiceCollection services,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null)
    {
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
        
        services.Configure<RetryBehaviorOptions>(options =>
        {
            options.MaxRetryAttempts = maxRetryAttempts;
            options.DelayBetweenRetries = delayBetweenRetries ?? TimeSpan.FromMilliseconds(500);
        });
        
        return services;
    }

    /// <summary>
    /// Adds transaction behavior to the pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorTransactionBehavior(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return services;
    }

    /// <summary>
    /// Adds all common behaviors to the pipeline in the recommended order.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure behavior options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorBehaviors(
        this IServiceCollection services,
        Action<MediatorBehaviorOptions>? configureOptions = null)
    {
        var options = new MediatorBehaviorOptions();
        configureOptions?.Invoke(options);

        // Add behaviors in the recommended order
        services.AddMediatorLoggingBehavior();
        services.AddMediatorPerformanceBehavior(options.SlowRequestThreshold);
        services.AddMediatorValidationBehavior();
        services.AddMediatorCachingBehavior(options.DefaultCacheDuration);
        services.AddMediatorRetryBehavior(options.MaxRetryAttempts, options.DelayBetweenRetries);
        services.AddMediatorTransactionBehavior();

        return services;
    }
}

/// <summary>
/// Configuration options for mediator behaviors.
/// </summary>
public class MediatorBehaviorOptions
{
    public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromMilliseconds(1000);
    public TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan DelayBetweenRetries { get; set; } = TimeSpan.FromMilliseconds(500);
}

/// <summary>
/// Configuration options for performance behavior.
/// </summary>
public class PerformanceBehaviorOptions
{
    public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromMilliseconds(1000);
}

/// <summary>
/// Configuration options for caching behavior.
/// </summary>
public class CachingBehaviorOptions
{
    public TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Configuration options for retry behavior.
/// </summary>
public class RetryBehaviorOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan DelayBetweenRetries { get; set; } = TimeSpan.FromMilliseconds(500);
}
