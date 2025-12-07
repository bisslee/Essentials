using Biss.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.Mediator;

/// <summary>
/// Default mediator implementation that uses reflection for dispatch.
/// This is used as a fallback when the source generator is not available.
/// </summary>
public partial class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetRequiredService(handlerType);
            
            var handleMethod = handlerType.GetMethod("Handle");
            var task = (Task<Result<TResponse>>)handleMethod!.Invoke(handler, new object[] { request, cancellationToken })!;
            
            return await task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request of type {RequestType}", typeof(TResponse).Name);
            return Result<TResponse>.Failure(Error.Generic("HANDLER_ERROR", ex.Message));
        }
    }

    public async Task<Result<Unit>> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            var handler = _serviceProvider.GetRequiredService(handlerType);
            
            var handleMethod = handlerType.GetMethod("Handle");
            var task = (Task<Result<Unit>>)handleMethod!.Invoke(handler, new object[] { command, cancellationToken })!;
            
            return await task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command of type {CommandType}", command.GetType().Name);
            return Result<Unit>.Failure(Error.Generic("HANDLER_ERROR", ex.Message));
        }
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);
            
            var tasks = handlers.Cast<INotificationHandler<INotification>>()
                .Select(handler => handler.Handle(notification, cancellationToken));
            
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling notification of type {NotificationType}", notification.GetType().Name);
        }
    }

    public async Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        TimeSpan timeout, 
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            return await Send(request, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return Result<TResponse>.Failure(Error.Generic("TIMEOUT", $"Request timed out after {timeout.TotalMilliseconds}ms"));
        }
    }
}
