namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents a handler that processes a command without returning a response.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit> 
    where TCommand : ICommand { }

/// <summary>
/// Represents a handler that processes a command and returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> 
    where TCommand : ICommand<TResponse> { }
