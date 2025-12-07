namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T? value, Error? error)
    {
        _value = value;
        _error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => _error is null;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the operation failed.</exception>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value of failed result");

    /// <summary>
    /// Gets the error if the operation failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the operation was successful.</exception>
    public Error Error => IsFailure ? _error! : throw new InvalidOperationException("Cannot access error of successful result");

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value to wrap in the result.</param>
    /// <returns>A successful result containing the value.</returns>
    public static Result<T> Success(T value) => new(value, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed result containing the error.</returns>
    public static Result<T> Failure(Error error) => new(default, error);

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result containing the value.</returns>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed result containing the error.</returns>
    public static implicit operator Result<T>(Error error) => Failure(error);
}
