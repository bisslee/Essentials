# Melhorias Críticas - Código Sugerido
## Implementações Necessárias para Biss.Essentials

---

## 1. Implementação Completa de Biss.Mediator.Extensions.AspNetCore

### 1.1 MediatorControllerBase.cs

```csharp
using Biss.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Biss.Mediator.Extensions.AspNetCore;

/// <summary>
/// Base controller that provides Mediator integration for ASP.NET Core.
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
    /// Maps an error to an appropriate HTTP action result.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>An action result representing the error.</returns>
    protected virtual ActionResult MapErrorToActionResult(Error error)
    {
        return error switch
        {
            NotFoundError => NotFound(new { error.Code, error.Message }),
            ValidationError validationError => BadRequest(new 
            { 
                error.Code, 
                error.Message,
                Field = validationError.Field 
            }),
            UnauthorizedError => Unauthorized(new { error.Code, error.Message }),
            _ => StatusCode(500, new { error.Code, error.Message })
        };
    }
}
```

### 1.2 ServiceCollectionExtensions.cs

```csharp
using Biss.Mediator.Abstractions;
using Biss.Mediator.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Biss.Mediator.Extensions.AspNetCore;

/// <summary>
/// Extension methods for registering Mediator services in ASP.NET Core.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mediator services and configures ASP.NET Core integration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorWithAspNetCore(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        // Register Mediator
        services.AddMediator(assemblies);
        
        // Configure MVC to use MediatorControllerBase
        services.Configure<MvcOptions>(options =>
        {
            // Adiciona suporte a MediatorControllerBase se necessário
        });
        
        return services;
    }
}
```

### 1.3 Atualizar Biss.Mediator.Extensions.AspNetCore.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Core" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
    <ProjectReference Include="..\Biss.Mediator.Extensions.DependencyInjection\Biss.Mediator.Extensions.DependencyInjection.csproj" />
  </ItemGroup>

</Project>
```

---

## 2. Integração com Biss.MultiSinkLogger

### 2.1 Atualizar LoggingBehavior.cs

```csharp
using Biss.Mediator.Abstractions;
using Biss.MultiSinkLogger; // Adicionar referência
using System.Diagnostics;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that logs request execution details using Biss.MultiSinkLogger.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMultiSinkLogger _logger; // Mudança aqui

    public LoggingBehavior(IMultiSinkLogger logger) // Mudança aqui
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation(
            "Starting request {RequestName} with ID {RequestId}",
            requestName, requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next(request, cancellationToken);
            
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Completed request {RequestName} with ID {RequestId} in {ElapsedMs}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Failed request {RequestName} with ID {RequestId} in {ElapsedMs}ms. Error: {ErrorCode} - {ErrorMessage}",
                    requestName, requestId, stopwatch.ElapsedMilliseconds, 
                    result.Error.Code, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Exception in request {RequestName} with ID {RequestId} after {ElapsedMs}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);

            return Result<TResponse>.Failure(Error.Generic("REQUEST_EXCEPTION", ex.Message));
        }
    }
}
```

### 2.2 Atualizar Biss.Mediator.Behaviors.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Options" />
    <!-- Adicionar referência ao Biss.MultiSinkLogger -->
    <PackageReference Include="Biss.MultiSinkLogger" Version="1.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Biss.Mediator.Abstractions\Biss.Mediator.Abstractions.csproj" />
  </ItemGroup>

</Project>
```

---

## 3. Integração com FluentValidation

### 3.1 Atualizar ValidationBehavior.cs

```csharp
using Biss.Mediator.Abstractions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Biss.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation and Data Annotations.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(
        ILogger<ValidationBehavior<TRequest, TResponse>> logger,
        IEnumerable<IValidator<TRequest>> validators)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validators = validators ?? Enumerable.Empty<IValidator<TRequest>>();
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Validação com FluentValidation
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => !r.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                _logger.LogWarning(
                    "FluentValidation failed for request {RequestType}: {ValidationErrors}",
                    typeof(TRequest).Name,
                    string.Join(", ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

                var errors = failures.Select(f =>
                    Error.Validation(f.PropertyName, f.ErrorMessage)).ToList();

                // Se houver múltiplos erros, retornar o primeiro ou criar um erro agregado
                if (errors.Count == 1)
                {
                    return Result<TResponse>.Failure(errors.First());
                }
                else
                {
                    // Para múltiplos erros, criar um erro agregado
                    var errorMessages = string.Join("; ", errors.Select(e => e.Message));
                    return Result<TResponse>.Failure(
                        Error.Validation("REQUEST_VALIDATION", errorMessages));
                }
            }
        }

        // Validação com Data Annotations (fallback)
        var dataAnnotationResult = ValidateWithDataAnnotations(request);
        if (!dataAnnotationResult.IsValid)
        {
            _logger.LogWarning(
                "Data Annotations validation failed for request {RequestType}: {ValidationErrors}",
                typeof(TRequest).Name,
                string.Join(", ", dataAnnotationResult.Errors.Select(e => e.ErrorMessage)));

            var error = dataAnnotationResult.Errors.FirstOrDefault();
            if (error != null)
            {
                return Result<TResponse>.Failure(
                    Error.Validation(error.PropertyName, error.ErrorMessage));
            }
        }

        return await next(request, cancellationToken);
    }

    private static ValidationResult ValidateWithDataAnnotations(TRequest request)
    {
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            request, validationContext, validationResults, true);
        
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
```

### 3.2 Atualizar ServiceCollectionExtensions.cs (Behaviors)

```csharp
// Adicionar método para registrar validação com FluentValidation
public static IServiceCollection AddMediatorValidationBehavior(
    this IServiceCollection services,
    params System.Reflection.Assembly[] assemblies)
{
    // Registrar validadores FluentValidation
    if (assemblies.Length > 0)
    {
        services.AddValidatorsFromAssemblies(assemblies);
    }
    
    services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    return services;
}
```

### 3.3 Atualizar Biss.Mediator.Behaviors.csproj

```xml
<ItemGroup>
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
    <!-- ... outras referências ... -->
</ItemGroup>
```

---

## 4. Melhorias no Result<T> para Múltiplos Erros

### 4.1 Atualizar Result.cs

```csharp
namespace Biss.Mediator.Abstractions;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with errors.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly IReadOnlyList<Error>? _errors;

    private Result(T? value, Error? error, IReadOnlyList<Error>? errors = null)
    {
        _value = value;
        _error = error;
        _errors = errors;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => _error is null && (_errors is null || _errors.Count == 0);

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
    /// Gets the first error if the operation failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the operation was successful.</exception>
    public Error Error => IsFailure ? (_error ?? _errors![0]) : throw new InvalidOperationException("Cannot access error of successful result");

    /// <summary>
    /// Gets all errors if the operation failed.
    /// </summary>
    public IReadOnlyList<Error> Errors => IsFailure 
        ? (_errors ?? (_error != null ? new[] { _error } : Array.Empty<Error>()))
        : Array.Empty<Error>();

    /// <summary>
    /// Gets a value indicating whether there are multiple errors.
    /// </summary>
    public bool HasMultipleErrors => _errors != null && _errors.Count > 1;

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value to wrap in the result.</param>
    /// <returns>A successful result containing the value.</returns>
    public static Result<T> Success(T value) => new(value, null, null);

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed result containing the error.</returns>
    public static Result<T> Failure(Error error) => new(default, error, null);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The errors that caused the failure.</param>
    /// <returns>A failed result containing the errors.</returns>
    public static Result<T> Failure(IReadOnlyList<Error> errors)
    {
        if (errors == null || errors.Count == 0)
            throw new ArgumentException("At least one error is required", nameof(errors));
        
        return new(default, errors[0], errors);
    }

    // ... resto dos métodos existentes ...
}
```

---

## 5. Correção do Source Generator do Mediator

### 5.1 Atualizar Mediator.cs para usar partial class

```csharp
using Biss.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.Mediator;

/// <summary>
/// Default mediator implementation that uses source-generated code for dispatch.
/// Falls back to reflection only if source generator is not available.
/// </summary>
public partial class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Métodos gerados pelo Source Generator serão injetados aqui como partial methods
    // Se não estiverem disponíveis, os métodos abaixo serão usados como fallback

    public async Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
    {
        // Tentar usar método gerado primeiro
        if (TrySendGenerated(request, cancellationToken, out var result))
        {
            return result;
        }

        // Fallback para reflection apenas se necessário
        return await SendWithReflection<TResponse>(request, cancellationToken);
    }

    // Partial method que será implementado pelo Source Generator
    partial bool TrySendGenerated<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken,
        out Task<Result<TResponse>> result);

    private async Task<Result<TResponse>> SendWithReflection<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetRequiredService(handlerType);
            
            var handleMethod = handlerType.GetMethod("Handle");
            var task = (Task<Result<TResponse>>)handleMethod!.Invoke(handler, new object[] { request, cancellationToken })!;
            
            return await task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request of type {RequestType}", typeof(TResponse).Name);
            return Result<TResponse>.Failure(Error.Generic("HANDLER_ERROR", ex.Message));
        }
    }

    // ... resto dos métodos ...
}
```

---

## 6. Atualizar Directory.Packages.props

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- ... outras dependências ... -->
    
    <!-- Adicionar Biss.MultiSinkLogger -->
    <PackageVersion Include="Biss.MultiSinkLogger" Version="1.0.0" />
    
    <!-- FluentValidation já está presente -->
    <PackageVersion Include="FluentValidation" Version="11.8.1" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.8.1" />
  </ItemGroup>
</Project>
```

---

## Checklist de Implementação

- [ ] Implementar `MediatorControllerBase` completo
- [ ] Implementar `ServiceCollectionExtensions` para ASP.NET Core
- [ ] Atualizar `LoggingBehavior` para usar `Biss.MultiSinkLogger`
- [ ] Adicionar referência ao `Biss.MultiSinkLogger` no projeto Behaviors
- [ ] Atualizar `ValidationBehavior` para suportar FluentValidation
- [ ] Adicionar referências ao FluentValidation
- [ ] Melhorar `Result<T>` para suportar múltiplos erros
- [ ] Corrigir Source Generator do Mediator
- [ ] Atualizar `Directory.Packages.props` com novas dependências
- [ ] Criar testes de integração para todas as funcionalidades
- [ ] Atualizar documentação

---

**Última atualização:** 2025-01-27

