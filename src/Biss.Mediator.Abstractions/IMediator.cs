namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents the main mediator interface for sending requests and publishing notifications.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command that does not return a response.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    Task<Result<Unit>> Send(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request with a custom timeout.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="timeout">The timeout for the operation.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        TimeSpan timeout, 
        CancellationToken cancellationToken = default);
}
