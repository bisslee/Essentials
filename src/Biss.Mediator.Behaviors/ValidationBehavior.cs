using Biss.Mediator.Abstractions;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests using Data Annotations.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request);
        
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for request {RequestType}: {ValidationErrors}", 
                typeof(TRequest).Name, string.Join(", ", validationResult.Errors));

            var errors = validationResult.Errors.Select(error => 
                Error.Validation(error.PropertyName, error.ErrorMessage)).ToList();

            return Result<TResponse>.Failure(Error.Validation("REQUEST_VALIDATION", 
                $"Validation failed: {string.Join(", ", errors.Select(e => e.Message))}"));
        }

        return await next(request, cancellationToken);
    }

    private static ValidationResult ValidateRequest(TRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);
        
        return new ValidationResult
        {
            IsValid = isValid,
            Errors = validationResults.Select(vr => new ValidationError
            {
                PropertyName = string.Join(", ", vr.MemberNames),
                ErrorMessage = vr.ErrorMessage ?? "Validation error"
            }).ToList()
        };
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
    }

    private class ValidationError
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
