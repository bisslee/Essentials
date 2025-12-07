namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
public abstract record Error(string Code, string Message)
{
    /// <summary>
    /// Creates a "not found" error for the specified resource.
    /// </summary>
    /// <param name="resource">The name of the resource that was not found.</param>
    /// <returns>A NotFoundError instance.</returns>
    public static Error NotFound(string resource) => new NotFoundError(resource);

    /// <summary>
    /// Creates a validation error for the specified field.
    /// </summary>
    /// <param name="field">The name of the field that failed validation.</param>
    /// <param name="message">The validation error message.</param>
    /// <returns>A ValidationError instance.</returns>
    public static Error Validation(string field, string message) => new ValidationError(field, message);

    /// <summary>
    /// Creates an unauthorized access error.
    /// </summary>
    /// <returns>An UnauthorizedError instance.</returns>
    public static Error Unauthorized() => new UnauthorizedError();

    /// <summary>
    /// Creates a generic error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A GenericError instance.</returns>
    public static Error Generic(string code, string message) => new GenericError(code, message);
}

/// <summary>
/// Represents an error when a resource is not found.
/// </summary>
/// <param name="Resource">The name of the resource that was not found.</param>
public record NotFoundError(string Resource) : Error("NOT_FOUND", $"Resource '{Resource}' not found");

/// <summary>
/// Represents a validation error.
/// </summary>
/// <param name="Field">The name of the field that failed validation.</param>
/// <param name="Message">The validation error message.</param>
public record ValidationError(string Field, string Message) : Error("VALIDATION", $"Field '{Field}': {Message}");

/// <summary>
/// Represents an unauthorized access error.
/// </summary>
public record UnauthorizedError() : Error("UNAUTHORIZED", "Access denied");

/// <summary>
/// Represents a generic error.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The error message.</param>
public record GenericError(string Code, string Message) : Error(Code, Message);
