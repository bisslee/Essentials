using Biss.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Biss.Mediator.Extensions.AspNetCore;

/// <summary>
/// Base controller that provides Mediator integration for ASP.NET Core.
/// This controller simplifies sending requests and commands through the mediator pattern
/// and automatically maps results to appropriate HTTP responses.
/// </summary>
public abstract class MediatorControllerBase : ControllerBase
{
    /// <summary>
    /// Gets the mediator instance.
    /// </summary>
    protected IMediator Mediator { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorControllerBase"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when mediator is null.</exception>
    protected MediatorControllerBase(IMediator mediator)
    {
        Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Sends a request and returns an appropriate HTTP response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An action result based on the request result.</returns>
    protected async Task<ActionResult<TResponse>> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(request, cancellationToken);
        
        if (result.IsSuccess)
            return Ok(result.Value);
        
        return MapErrorToActionResult(result.Error);
    }

    /// <summary>
    /// Sends a command that does not return a response.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An action result indicating success or failure.</returns>
    protected async Task<ActionResult> Send(
        ICommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        if (result.IsSuccess)
            return Ok();
        
        return MapErrorToActionResult(result.Error);
    }

    /// <summary>
    /// Sends a command that returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An action result based on the command result.</returns>
    protected async Task<ActionResult<TResponse>> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        if (result.IsSuccess)
            return Ok(result.Value);
        
        return MapErrorToActionResult(result.Error);
    }

    /// <summary>
    /// Sends a request and returns a response with a custom status code on success.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="successStatusCode">The HTTP status code to return on success.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An action result based on the request result.</returns>
    protected async Task<ActionResult<TResponse>> Send<TResponse>(
        IRequest<TResponse> request,
        int successStatusCode,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(request, cancellationToken);
        
        if (result.IsSuccess)
            return StatusCode(successStatusCode, result.Value);
        
        return MapErrorToActionResult(result.Error);
    }

    /// <summary>
    /// Sends a request and returns a response with a custom status code on success.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="successStatusCode">The HTTP status code to return on success.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An action result based on the request result.</returns>
    protected async Task<ActionResult<TResponse>> Send<TResponse>(
        IRequest<TResponse> request,
        System.Net.HttpStatusCode successStatusCode,
        CancellationToken cancellationToken = default)
    {
        return await Send(request, (int)successStatusCode, cancellationToken);
    }

    /// <summary>
    /// Sends a command and returns a response with a custom status code on success.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="successStatusCode">The HTTP status code to return on success.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An action result indicating success or failure.</returns>
    protected async Task<ActionResult> Send(
        ICommand command,
        int successStatusCode,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        if (result.IsSuccess)
            return StatusCode(successStatusCode);
        
        return MapErrorToActionResult(result.Error);
    }

    /// <summary>
    /// Sends a command and returns a response with a custom status code on success.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="successStatusCode">The HTTP status code to return on success.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An action result indicating success or failure.</returns>
    protected async Task<ActionResult> Send(
        ICommand command,
        System.Net.HttpStatusCode successStatusCode,
        CancellationToken cancellationToken = default)
    {
        return await Send(command, (int)successStatusCode, cancellationToken);
    }

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async Task Publish(
        INotification notification,
        CancellationToken cancellationToken = default)
    {
        await Mediator.Publish(notification, cancellationToken);
    }

    /// <summary>
    /// Maps an error to an appropriate HTTP action result.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>An action result representing the error.</returns>
    protected virtual ActionResult MapErrorToActionResult(Error error)
    {
        return error switch
        {
            NotFoundError => NotFound(new ErrorResponse
            {
                Code = error.Code,
                Message = error.Message
            }),
            ValidationError validationError => BadRequest(new ValidationErrorResponse
            {
                Code = error.Code,
                Message = error.Message,
                Field = validationError.Field
            }),
            UnauthorizedError => Unauthorized(new ErrorResponse
            {
                Code = error.Code,
                Message = error.Message
            }),
            _ => StatusCode(500, new ErrorResponse
            {
                Code = error.Code,
                Message = error.Message
            })
        };
    }
}

/// <summary>
/// Represents an error response returned to the client.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents a validation error response returned to the client.
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Gets or sets the field name that failed validation.
    /// </summary>
    public string Field { get; set; } = string.Empty;
}

