using Biss.Mediator.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that measures and logs performance metrics.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _slowRequestThreshold;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        TimeSpan? slowRequestThreshold = null)
    {
        _logger = logger;
        _slowRequestThreshold = slowRequestThreshold ?? TimeSpan.FromMilliseconds(1000);
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next(request, cancellationToken);
            stopwatch.Stop();

            var elapsed = stopwatch.Elapsed;

            if (elapsed > _slowRequestThreshold)
            {
                _logger.LogWarning(
                    "Slow request detected: {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                    requestName, elapsed.TotalMilliseconds, _slowRequestThreshold.TotalMilliseconds);
            }
            else
            {
                _logger.LogDebug(
                    "Request {RequestName} completed in {ElapsedMs}ms",
                    requestName, elapsed.TotalMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Request {RequestName} failed after {ElapsedMs}ms",
                requestName, stopwatch.Elapsed.TotalMilliseconds);

            throw;
        }
    }
}
