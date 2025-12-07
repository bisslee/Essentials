# Especificação Técnica Melhorada: Componentes Mediator e Mapper para .NET 9

**Autor:** Biss Lee  
**Data:** 24 de outubro de 2025  
**Versão:** 2.0  
**Status:** Em Desenvolvimento

## 1. Introdução

Este documento apresenta uma especificação técnica aprimorada para a criação de componentes **Mediator** e **Mapper** para ecossistemas .NET 9, desenvolvidos como alternativas de alta performance aos pacotes comerciais AutoMapper e MediatR. A solução proposta utiliza **Source Generators** para eliminar completamente o uso de reflexão em tempo de execução, garantindo máxima performance, segurança de tipos e compatibilidade total com compilação AOT (Ahead-of-Time) e trimming.

### 1.1. Objetivos

- **Substituir dependências comerciais**: Criar alternativas open-source aos pacotes AutoMapper e MediatR
- **Performance superior**: Utilizar Source Generators para eliminar overhead de reflexão
- **Facilidade de migração**: APIs similares às bibliotecas originais para facilitar a transição
- **Compatibilidade AOT**: Suporte completo para compilação Ahead-of-Time
- **Adesão aos princípios SOLID**: Arquitetura limpa e extensível

### 1.2. Referências e Inspirações

- **Mapperly**: Biblioteca de mapeamento baseada em Source Generators
- **FastEndpoints**: Framework moderno para APIs com padrão Mediator
- **Mapster**: Alternativa performática ao AutoMapper
- **MediatR**: Padrão de referência para implementação do Mediator Pattern

## 2. Princípios de Design Aprimorados

| Princípio | Descrição | Implementação |
|-----------|-----------|---------------|
| **Performance Máxima** | Zero reflexão em runtime através de Source Generators | Geração de código otimizado em tempo de compilação |
| **Type Safety** | Validação completa de tipos em tempo de compilação | Source Generators com análise semântica |
| **Clean Code & SOLID** | Arquitetura modular seguindo princípios SOLID | Separação clara de responsabilidades |
| **Developer Experience** | APIs intuitivas com IntelliSense completo | Métodos de extensão e atributos bem documentados |
| **Extensibilidade** | Pipeline de comportamentos e conversores customizados | Interfaces bem definidas para extensões |
| **AOT Compatibility** | Suporte completo para compilação Ahead-of-Time | Zero dependências de reflexão |
| **Migration Path** | Facilidade de migração de AutoMapper/MediatR | APIs similares e ferramentas de migração |

## 3. Componente Mediator Aprimorado

### 3.1. Arquitetura Modular

O componente Mediator será dividido em módulos especializados:

```
Biss.Mediator/
├── Abstractions/           # Interfaces e contratos
├── Core/                   # Implementação principal
├── Behaviors/              # Pipeline behaviors
├── Extensions/             # Extensões para DI e ASP.NET Core
├── SourceGenerators/       # Source Generators
└── Diagnostics/            # Ferramentas de diagnóstico
```

### 3.2. Abstrações (`Biss.Mediator.Abstractions`)

#### 3.2.1. Sistema de Resultados Robusto

```csharp
namespace Biss.Mediator.Abstractions;

// Resultado genérico para encapsular sucesso/erro
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    
    public bool IsSuccess => _error is null;
    public bool IsFailure => !IsSuccess;
    
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value of failed result");
    public Error Error => IsFailure ? _error! : throw new InvalidOperationException("Cannot access error of successful result");
    
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);
}

// Sistema de erros tipado
public abstract record Error(string Code, string Message)
{
    public static Error NotFound(string resource) => new NotFoundError(resource);
    public static Error Validation(string field, string message) => new ValidationError(field, message);
    public static Error Unauthorized() => new UnauthorizedError();
}

public record NotFoundError(string Resource) : Error("NOT_FOUND", $"Resource '{Resource}' not found");
public record ValidationError(string Field, string Message) : Error("VALIDATION", $"Field '{Field}': {Message}");
public record UnauthorizedError() : Error("UNAUTHORIZED", "Access denied");
```

#### 3.2.2. Mensagens Aprimoradas

```csharp
namespace Biss.Mediator.Abstractions;

// Interface base para todas as mensagens
public interface IRequest<TResponse> { }

// Comandos com e sem retorno
public interface ICommand : IRequest<Unit> { }
public interface ICommand<TResponse> : IRequest<TResponse> { }

// Queries sempre retornam dados
public interface IQuery<TResponse> : IRequest<TResponse> { }

// Notificações para eventos
public interface INotification { }

// Unit type para comandos sem retorno
public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = new();
    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
}
```

#### 3.2.3. Handlers com Melhor Ergonomia

```csharp
namespace Biss.Mediator.Abstractions;

// Handler base com Result<T>
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default);
}

// Handlers específicos
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit> 
    where TCommand : ICommand { }

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> 
    where TCommand : ICommand<TResponse> { }

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> 
    where TQuery : IQuery<TResponse> { }

public interface INotificationHandler<in TNotification> 
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}
```

#### 3.2.4. Pipeline Behaviors Aprimorados

```csharp
namespace Biss.Mediator.Abstractions;

// Behavior base com melhor ergonomia
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> Handle(
        TRequest request, 
        RequestHandlerDelegate<TRequest, TResponse> next, 
        CancellationToken cancellationToken);
}

// Delegate para o próximo handler
public delegate Task<Result<TResponse>> RequestHandlerDelegate<TRequest, TResponse>(
    TRequest request, 
    CancellationToken cancellationToken);

// Behaviors pré-implementados
public interface IValidationBehavior<in TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> { }

public interface ILoggingBehavior<in TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> { }

public interface ICachingBehavior<in TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> { }
```

#### 3.2.5. Interface Principal do Mediator

```csharp
namespace Biss.Mediator.Abstractions;

public interface IMediator
{
    // Envio de requests com Result<T>
    Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default);
    
    // Envio de comandos sem retorno
    Task<Result<Unit>> Send(ICommand command, CancellationToken cancellationToken = default);
    
    // Publicação de notificações
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
    
    // Envio com timeout customizado
    Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        TimeSpan timeout, 
        CancellationToken cancellationToken = default);
}
```

### 3.3. Implementação com Source Generators (`Biss.Mediator`)

#### 3.3.1. Source Generator para Dispatch Otimizado

O Source Generator irá:

1. **Escanear assemblies** em busca de handlers implementando as interfaces
2. **Gerar código otimizado** com switch expressions para dispatch direto
3. **Criar métodos de registro** automático para DI
4. **Emitir diagnósticos** para handlers não registrados

```csharp
// Código gerado pelo Source Generator
public partial class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task<Result<TResponse>> Send<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
    {
        return request switch
        {
            CreateUserCommand cmd => HandleCommand(cmd, cancellationToken),
            UpdateUserCommand cmd => HandleCommand(cmd, cancellationToken),
            GetUserQuery query => HandleQuery(query, cancellationToken),
            _ => Task.FromResult(Result<TResponse>.Failure(Error.NotFound($"Handler for {typeof(TResponse).Name}")))
        };
    }

    private async Task<Result<TResponse>> HandleCommand<TCommand, TResponse>(
        TCommand command, 
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        return await handler.Handle(command, cancellationToken);
    }

    private async Task<Result<TResponse>> HandleQuery<TQuery, TResponse>(
        TQuery query, 
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return await handler.Handle(query, cancellationToken);
    }
}
```

### 3.4. Behaviors Pré-implementados (`Biss.Mediator.Behaviors`)

#### 3.4.1. Validation Behavior

```csharp
namespace Biss.Mediator.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IValidationBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request, 
        RequestHandlerDelegate<TRequest, TResponse> next, 
        CancellationToken cancellationToken)
    {
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .ToList();

        if (errors.Any())
        {
            return Result<TResponse>.Failure(
                new ValidationError("Request", string.Join("; ", errors.Select(e => e.ErrorMessage))));
        }

        return await next(request, cancellationToken);
    }
}
```

#### 3.4.2. Logging Behavior

```csharp
namespace Biss.Mediator.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : ILoggingBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request, 
        RequestHandlerDelegate<TRequest, TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        _logger.LogInformation("Handling {RequestName} with ID {RequestId}", requestName, requestId);

        try
        {
            var result = await next(request, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully handled {RequestName} with ID {RequestId}", requestName, requestId);
            }
            else
            {
                _logger.LogWarning("Failed to handle {RequestName} with ID {RequestId}: {Error}", 
                    requestName, requestId, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName} with ID {RequestId}", requestName, requestId);
            throw;
        }
    }
}
```

## 4. Componente Mapper Aprimorado

### 4.1. Arquitetura Inspirada no Mapperly

```
Biss.Mapper/
├── Abstractions/           # Atributos e interfaces
├── Core/                   # Implementação principal
├── Extensions/             # Extensões para DI
├── SourceGenerators/       # Source Generators
└── Diagnostics/            # Ferramentas de diagnóstico
```

### 4.2. Abstrações (`Biss.Mapper.Abstractions`)

#### 4.2.1. Atributos para Configuração

```csharp
namespace Biss.Mapper.Abstractions;

// Atributo principal para marcar classes de mapeamento
[AttributeUsage(AttributeTargets.Class)]
public sealed class MapperAttribute : Attribute
{
    public bool IgnoreNullValues { get; set; } = false;
    public bool IgnoreNullCollections { get; set; } = false;
    public bool UseDeepCopy { get; set; } = false;
}

// Atributo para mapeamento de propriedades específicas
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class MapPropertyAttribute : Attribute
{
    public string SourceProperty { get; }
    public string? TargetProperty { get; set; }
    public bool Ignore { get; set; } = false;
    public string? Converter { get; set; }

    public MapPropertyAttribute(string sourceProperty)
    {
        SourceProperty = sourceProperty;
    }
}

// Atributo para ignorar propriedades
[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreAttribute : Attribute { }

// Atributo para mapeamento condicional
[AttributeUsage(AttributeTargets.Method)]
public sealed class MapConditionAttribute : Attribute
{
    public string Condition { get; }

    public MapConditionAttribute(string condition)
    {
        Condition = condition;
    }
}
```

#### 4.2.2. Interfaces para Extensibilidade

```csharp
namespace Biss.Mapper.Abstractions;

// Interface para conversores customizados
public interface ITypeConverter<TSource, TDestination>
{
    TDestination Convert(TSource source, IMappingContext context);
}

// Interface para contexto de mapeamento
public interface IMappingContext
{
    IServiceProvider ServiceProvider { get; }
    IDictionary<string, object> Items { get; }
}

// Interface para configuração de mapeamento
public interface IMappingConfiguration
{
    void ConfigureMapping<TSource, TDestination>(IMappingProfile<TSource, TDestination> profile);
}

// Interface para perfil de mapeamento
public interface IMappingProfile<TSource, TDestination>
{
    IMappingProfile<TSource, TDestination> Map<TProperty>(
        Expression<Func<TSource, TProperty>> sourceMember,
        Expression<Func<TDestination, TProperty>> destinationMember);

    IMappingProfile<TSource, TDestination> Ignore<TProperty>(
        Expression<Func<TDestination, TProperty>> destinationMember);

    IMappingProfile<TSource, TDestination> UseConverter<TConverter>()
        where TConverter : ITypeConverter<TSource, TDestination>;
}
```

### 4.3. Implementação com Source Generators (`Biss.Mapper`)

#### 4.3.1. Exemplo de Uso Simplificado

```csharp
// Definição dos modelos
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Address Address { get; set; } = new();
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

// Definição do Mapper
[Mapper]
public partial class UserMapper
{
    // Mapeamento simples
    public partial UserDto ToDto(User user);

    // Mapeamento com configuração customizada
    [MapProperty("FirstName", "FullName")]
    public partial UserDto ToDtoWithCustomMapping(User user);

    // Mapeamento de coleções
    public partial IEnumerable<UserDto> ToDtoCollection(IEnumerable<User> users);

    // Mapeamento reverso
    public partial User FromDto(UserDto dto);
}
```

#### 4.3.2. Código Gerado pelo Source Generator

```csharp
// Código gerado automaticamente
public partial class UserMapper
{
    public partial UserDto ToDto(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return new UserDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            AddressLine = $"{user.Address.Street}, {user.Address.City}, {user.Address.Country}",
            CreatedAt = user.CreatedAt
        };
    }

    public partial UserDto ToDtoWithCustomMapping(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FirstName, // Mapeamento customizado
            Email = user.Email,
            AddressLine = $"{user.Address.Street}, {user.Address.City}, {user.Address.Country}",
            CreatedAt = user.CreatedAt
        };
    }

    public partial IEnumerable<UserDto> ToDtoCollection(IEnumerable<User> users)
    {
        if (users == null)
            throw new ArgumentNullException(nameof(users));

        return users.Select(ToDto);
    }

    public partial User FromDto(UserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Parse do FullName para FirstName e LastName
        var nameParts = dto.FullName.Split(' ', 2);
        
        return new User
        {
            Id = dto.Id,
            FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
            LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            Email = dto.Email,
            CreatedAt = dto.CreatedAt,
            Address = ParseAddress(dto.AddressLine)
        };
    }

    private static Address ParseAddress(string addressLine)
    {
        var parts = addressLine.Split(',');
        return new Address
        {
            Street = parts.Length > 0 ? parts[0].Trim() : string.Empty,
            City = parts.Length > 1 ? parts[1].Trim() : string.Empty,
            Country = parts.Length > 2 ? parts[2].Trim() : string.Empty
        };
    }
}
```

### 4.4. Extensões para Injeção de Dependência

```csharp
namespace Biss.Mapper.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMappers(
        this IServiceCollection services, 
        Assembly assembly)
    {
        // Registro automático de todos os mappers
        var mapperTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<MapperAttribute>() != null)
            .ToList();

        foreach (var mapperType in mapperTypes)
        {
            services.AddSingleton(mapperType);
        }

        // Registro de conversores customizados
        var converterTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeConverter<,>)))
            .ToList();

        foreach (var converterType in converterTypes)
        {
            var interfaceType = converterType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeConverter<,>));
            
            services.AddSingleton(interfaceType, converterType);
        }

        return services;
    }

    public static IServiceCollection AddMappersFromAssemblyContaining<T>(
        this IServiceCollection services)
    {
        return services.AddMappers(typeof(T).Assembly);
    }
}
```

## 5. Aplicação dos Princípios SOLID Aprimorada

| Princípio | Aplicação no Mediator | Aplicação no Mapper |
|-----------|----------------------|-------------------|
| **Single Responsibility** | Cada handler processa apenas um tipo de mensagem. Behaviors têm responsabilidades específicas (logging, validation, caching). | Cada mapper é responsável por um conjunto coeso de mapeamentos. Conversores lidam com conversões específicas. |
| **Open/Closed** | Sistema aberto para novos handlers e behaviors, fechado para modificação do núcleo. | Novos mapeamentos e conversores podem ser adicionados sem alterar a engine. |
| **Liskov Substitution** | Handlers e behaviors podem ser substituídos por implementações alternativas. | Conversores podem ser substituídos por implementações customizadas. |
| **Interface Segregation** | Interfaces granulares (`ICommand`, `IQuery`, `INotification`) evitam dependências desnecessárias. | Interfaces específicas para diferentes tipos de conversão e configuração. |
| **Dependency Inversion** | Código depende de abstrações (`IMediator`, `IRequestHandler`), não de implementações concretas. | Mapeamentos dependem de abstrações (`ITypeConverter`, `IMappingContext`). |

## 6. Estrutura dos Pacotes NuGet Aprimorada

| Pacote | Dependências | Descrição |
|--------|-------------|-----------|
| `Biss.Mediator.Abstractions` | - | Interfaces, contratos e tipos base |
| `Biss.Mediator` | `Biss.Mediator.Abstractions` | Implementação principal com Source Generator |
| `Biss.Mediator.Behaviors` | `Biss.Mediator.Abstractions` | Behaviors pré-implementados (validation, logging, caching) |
| `Biss.Mediator.Extensions.DependencyInjection` | `Biss.Mediator` | Extensões para registro automático no DI |
| `Biss.Mediator.Extensions.AspNetCore` | `Biss.Mediator.Extensions.DependencyInjection` | Integração com ASP.NET Core |
| `Biss.Mapper.Abstractions` | - | Atributos e interfaces para mapeamento |
| `Biss.Mapper` | `Biss.Mapper.Abstractions` | Implementação principal com Source Generator |
| `Biss.Mapper.Extensions.DependencyInjection` | `Biss.Mapper` | Extensões para registro automático no DI |

## 7. Ferramentas de Migração

### 7.1. Migração do AutoMapper

```csharp
// Ferramenta de análise para identificar mapeamentos AutoMapper
public class AutoMapperAnalyzer
{
    public MigrationReport AnalyzeProject(string projectPath)
    {
        // Análise de código para identificar:
        // - Profile classes
        // - CreateMap calls
        // - Map calls
        // - Custom resolvers
        // - Value resolvers
    }
}

// Gerador de código para migração
public class AutoMapperMigrationGenerator
{
    public void GenerateMapperClasses(MigrationReport report, string outputPath)
    {
        // Geração automática de classes mapper baseadas nos profiles existentes
    }
}
```

### 7.2. Migração do MediatR

```csharp
// Ferramenta de análise para identificar handlers MediatR
public class MediatRAnalyzer
{
    public MediatRMigrationReport AnalyzeProject(string projectPath)
    {
        // Análise de código para identificar:
        // - IRequest implementations
        // - IRequestHandler implementations
        // - INotification implementations
        // - INotificationHandler implementations
    }
}
```

## 8. Roadmap de Implementação

### Fase 1: Fundação (Semanas 1-2)
- [ ] Criar estrutura de projetos
- [ ] Implementar abstrações básicas
- [ ] Configurar Source Generators
- [ ] Implementar testes unitários básicos

### Fase 2: Mediator Core (Semanas 3-4)
- [ ] Implementar Source Generator para Mediator
- [ ] Criar behaviors básicos (logging, validation)
- [ ] Implementar sistema de Result<T>
- [ ] Criar extensões para DI

### Fase 3: Mapper Core (Semanas 5-6)
- [ ] Implementar Source Generator para Mapper
- [ ] Criar sistema de atributos
- [ ] Implementar conversores customizados
- [ ] Criar extensões para DI

### Fase 4: Ferramentas e Documentação (Semanas 7-8)
- [ ] Implementar ferramentas de migração
- [ ] Criar documentação completa
- [ ] Implementar exemplos práticos
- [ ] Criar benchmarks de performance

### Fase 5: Testes e Refinamento (Semanas 9-10)
- [ ] Testes de integração
- [ ] Testes de performance
- [ ] Refinamento baseado em feedback
- [ ] Preparação para release

## 9. Considerações de Performance

### 9.1. Benchmarks Esperados

| Operação | AutoMapper | Biss.Mapper | Melhoria |
|----------|------------|-------------|----------|
| Mapeamento Simples | 100ns | 15ns | 6.7x |
| Mapeamento Complexo | 500ns | 50ns | 10x |
| Coleções (1000 items) | 50ms | 5ms | 10x |
| Startup Time | 200ms | 10ms | 20x |

### 9.2. Otimizações Implementadas

- **Zero Reflection**: Source Generators eliminam reflexão em runtime
- **Inlining**: Métodos pequenos são inlined pelo JIT
- **Memory Pooling**: Reutilização de objetos para reduzir GC pressure
- **SIMD**: Otimizações SIMD para operações em coleções
- **AOT Compatibility**: Compilação Ahead-of-Time sem limitações

## 10. Conclusão

Esta especificação aprimorada apresenta uma arquitetura robusta e performática para componentes Mediator e Mapper que:

1. **Eliminam dependências comerciais** através de alternativas open-source
2. **Maximizam performance** utilizando Source Generators
3. **Garantem facilidade de migração** com APIs similares
4. **Seguem princípios SOLID** para código limpo e extensível
5. **Suportam AOT** para aplicações modernas

A implementação seguirá o roadmap proposto, garantindo qualidade, performance e facilidade de uso comparáveis ou superiores às bibliotecas comerciais existentes.
