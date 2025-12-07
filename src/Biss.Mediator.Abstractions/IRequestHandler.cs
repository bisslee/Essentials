namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents a handler that processes a request of type <typeparamref name="TRequest"/> and returns a result of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default);
}
