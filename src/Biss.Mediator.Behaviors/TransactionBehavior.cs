using Biss.Mediator.Abstractions;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command requests in a database transaction.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only wrap commands in transactions
        if (!IsCommandRequest(request))
        {
            return await next(request, cancellationToken);
        }

        var requestName = typeof(TRequest).Name;
        
        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(5)
            },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            _logger.LogDebug("Starting transaction for command {CommandName}", requestName);
            
            var result = await next(request, cancellationToken);
            
            if (result.IsSuccess)
            {
                transactionScope.Complete();
                _logger.LogDebug("Transaction completed successfully for command {CommandName}", requestName);
            }
            else
            {
                _logger.LogWarning("Transaction rolled back for command {CommandName} due to failure: {Error}", 
                    requestName, result.Error.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction rolled back for command {CommandName} due to exception", requestName);
            throw;
        }
    }

    private static bool IsCommandRequest(TRequest request)
    {
        // Check if the request implements ICommand
        var requestType = request.GetType();
        return requestType.GetInterfaces()
            .Any(i => i.Name == "ICommand");
    }
}
