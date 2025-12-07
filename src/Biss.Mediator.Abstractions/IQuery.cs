namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents a query that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse> { }
