namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents a command that does not return a response (fire-and-forget).
/// </summary>
public interface ICommand : IRequest { }

/// <summary>
/// Represents a command that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse> { }
