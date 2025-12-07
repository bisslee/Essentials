namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents a request that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
public interface IRequest<TResponse> { }

/// <summary>
/// Represents a request that does not return a response (fire-and-forget).
/// </summary>
public interface IRequest : IRequest<Unit> { }
