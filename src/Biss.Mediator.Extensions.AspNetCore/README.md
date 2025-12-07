# Biss.Mediator.Extensions.AspNetCore

Extensões do Biss.Mediator para integração com ASP.NET Core.

## 📦 Instalação

```bash
dotnet add package Biss.Mediator.Extensions.AspNetCore
```

## 🚀 Uso Rápido

### 1. Registrar o Mediator no Startup

```csharp
using Biss.Mediator.Extensions.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Adicionar Mediator com suporte a ASP.NET Core
builder.Services.AddMediatorWithAspNetCore(typeof(Program).Assembly);

// Ou especificar assemblies manualmente
builder.Services.AddMediatorWithAspNetCore(
    typeof(Program).Assembly,
    typeof(MyCommand).Assembly
);
```

### 2. Criar um Controller Baseado em Mediator

```csharp
using Biss.Mediator.Abstractions;
using Biss.Mediator.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : MediatorControllerBase
{
    public UsersController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<ActionResult<UserId>> CreateUser(CreateUserCommand command)
    {
        // O método Send automaticamente mapeia Result<T> para ActionResult<T>
        return await Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var query = new GetUserQuery(id);
        return await Send(query);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        return await Send(command);
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateUser(Guid id)
    {
        var command = new ActivateUserCommand(id);
        // Usar status code customizado
        return await Send(command, System.Net.HttpStatusCode.Accepted);
    }
}
```

### 3. Definir Commands e Queries

```csharp
// Command
public record CreateUserCommand(string FirstName, string LastName, string Email) 
    : ICommand<UserId>;

// Query
public record GetUserQuery(Guid Id) : IQuery<UserDto>;

// Command sem retorno
public record DeleteUserCommand(Guid Id) : ICommand;
```

### 4. Criar Handlers

```csharp
public class CreateUserHandler : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<Result<UserId>> Handle(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        // Sua lógica aqui
        var userId = new UserId(Guid.NewGuid());
        return Result<UserId>.Success(userId);
    }
}
```

## 📚 Recursos

### MediatorControllerBase

A classe base `MediatorControllerBase` fornece métodos auxiliares para enviar requests e commands:

- `Send<TResponse>(IRequest<TResponse>)` - Envia um request e retorna ActionResult<TResponse>
- `Send(ICommand)` - Envia um command sem retorno
- `Send<TResponse>(ICommand<TResponse>)` - Envia um command com retorno
- `Send<TResponse>(IRequest<TResponse>, int statusCode)` - Envia com status code customizado
- `Publish(INotification)` - Publica uma notificação

### Mapeamento Automático de Erros

O `MediatorControllerBase` mapeia automaticamente erros para códigos HTTP apropriados:

- `NotFoundError` → 404 Not Found
- `ValidationError` → 400 Bad Request
- `UnauthorizedError` → 401 Unauthorized
- Outros erros → 500 Internal Server Error

### HttpContext Extensions

Você também pode acessar o Mediator diretamente do HttpContext:

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;

    public MyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var mediator = context.GetMediator();
        // Usar o mediator aqui
        await _next(context);
    }
}
```

## 🔧 Configuração Avançada

### Configurar MVC Options

```csharp
builder.Services.ConfigureMediatorMvc(options =>
{
    // Configurar opções do MVC relacionadas ao Mediator
});
```

## 📖 Exemplos Completos

### Exemplo: CRUD Completo

```csharp
[ApiController]
[Route("api/customers")]
public class CustomersController : MediatorControllerBase
{
    public CustomersController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<ActionResult<CustomerId>> Create(CreateCustomerCommand command)
        => await Send(command, StatusCodes.Status201Created);

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> Get(Guid id)
        => await Send(new GetCustomerQuery(id));

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateCustomerCommand command)
        => await Send(command);

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
        => await Send(new DeleteCustomerCommand(id));

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> List(
        [FromQuery] GetCustomersQuery query)
        => await Send(query);
}
```

## 🎯 Benefícios

- ✅ **Type Safety**: Compile-time validation de requests e responses
- ✅ **Clean Code**: Controllers limpos e focados apenas em HTTP
- ✅ **Error Handling**: Mapeamento automático de erros para HTTP
- ✅ **Testability**: Fácil de testar com mocks do IMediator
- ✅ **Performance**: Source Generators eliminam reflection overhead

## 📄 Licença

Este projeto está licenciado sob a Licença MIT.

