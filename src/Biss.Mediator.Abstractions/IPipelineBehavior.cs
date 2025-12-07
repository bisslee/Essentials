namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents a delegate for the next handler in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
/// <param name="request">The request to process.</param>
/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
/// <returns>A task that represents the asynchronous operation and contains the result.</returns>
public delegate Task<Result<TResponse>> RequestHandlerDelegate<TRequest, TResponse>(
    TRequest request, 
    CancellationToken cancellationToken);

/// <summary>
/// Represents a behavior in the request processing pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request by calling the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="next">The delegate to call the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    Task<Result<TResponse>> Handle(
        TRequest request, 
        RequestHandlerDelegate<TRequest, TResponse> next, 
        CancellationToken cancellationToken);
}
