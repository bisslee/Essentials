using Biss.Mediator.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that logs request execution details.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation(
            "Starting request {RequestName} with ID {RequestId}",
            requestName, requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next(request, cancellationToken);
            
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Completed request {RequestName} with ID {RequestId} in {ElapsedMs}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Failed request {RequestName} with ID {RequestId} in {ElapsedMs}ms. Error: {Error}",
                    requestName, requestId, stopwatch.ElapsedMilliseconds, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Exception in request {RequestName} with ID {RequestId} after {ElapsedMs}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);

            return Result<TResponse>.Failure(Error.Generic("REQUEST_EXCEPTION", ex.Message));
        }
    }
}
