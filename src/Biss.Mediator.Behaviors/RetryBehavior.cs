using Biss.Mediator.Abstractions;
using Microsoft.Extensions.Logging;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that provides retry functionality for failed requests.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _delayBetweenRetries;

    public RetryBehavior(
        ILogger<RetryBehavior<TRequest, TResponse>> logger,
        int maxRetryAttempts = 3,
        TimeSpan? delayBetweenRetries = null)
    {
        _logger = logger;
        _maxRetryAttempts = maxRetryAttempts;
        _delayBetweenRetries = delayBetweenRetries ?? TimeSpan.FromMilliseconds(500);
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var lastException = (Exception?)null;

        for (var attempt = 1; attempt <= _maxRetryAttempts; attempt++)
        {
            try
            {
                var result = await next(request, cancellationToken);
                
                if (result.IsSuccess || attempt == _maxRetryAttempts)
                {
                    if (attempt > 1)
                    {
                        _logger.LogInformation(
                            "Request {RequestName} succeeded on attempt {Attempt}",
                            requestName, attempt);
                    }
                    
                    return result;
                }

                if (result.IsFailure)
                {
                    _logger.LogWarning(
                        "Request {RequestName} failed on attempt {Attempt}/{MaxAttempts}: {Error}",
                        requestName, attempt, _maxRetryAttempts, result.Error.Message);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                _logger.LogWarning(ex,
                    "Request {RequestName} threw exception on attempt {Attempt}/{MaxAttempts}",
                    requestName, attempt, _maxRetryAttempts);

                if (attempt == _maxRetryAttempts)
                {
                    break;
                }
            }

            if (attempt < _maxRetryAttempts)
            {
                var delay = TimeSpan.FromMilliseconds(_delayBetweenRetries.TotalMilliseconds * attempt);
                _logger.LogDebug(
                    "Waiting {DelayMs}ms before retry {NextAttempt} for request {RequestName}",
                    delay.TotalMilliseconds, attempt + 1, requestName);
                
                await Task.Delay(delay, cancellationToken);
            }
        }

        // If we get here, all retry attempts failed
        var errorMessage = lastException?.Message ?? "Request failed after all retry attempts";
        
        _logger.LogError(lastException,
            "Request {RequestName} failed after {MaxAttempts} attempts",
            requestName, _maxRetryAttempts);

        return Result<TResponse>.Failure(Error.Generic("RETRY_EXHAUSTED", errorMessage));
    }
}
