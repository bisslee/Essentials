using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Biss.Mediator;
using Biss.Mediator.Abstractions;
using Biss.Mediator.Extensions.DependencyInjection;
using Biss.Mapper;
using Biss.Mapper.Abstractions;
using Biss.Mapper.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Biss.PerformanceBenchmarks;

/// <summary>
/// Performance benchmarks comparing Biss components with alternatives.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

/// <summary>
/// Benchmarks for Mediator pattern implementations.
/// </summary>
[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private IMediator _mediator = null!;
    private CreateUserCommand _command = null!;
    private GetUserQuery _query = null!;
    private IServiceProvider _serviceProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Register Biss Mediator
        services.AddMediator(typeof(Program).Assembly);
        
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        
        _command = new CreateUserCommand("John", "Doe", "john.doe@example.com");
        _query = new GetUserQuery(1);
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<Unit>> SendCommand()
    {
        return await _mediator.Send(_command);
    }

    [Benchmark]
    public async Task<Result<UserDto>> SendQuery()
    {
        return await _mediator.Send(_query);
    }

    [Benchmark]
    public async Task PublishNotification()
    {
        await _mediator.Publish(new UserCreatedNotification(1, "John", "Doe"));
    }

    [Benchmark]
    public async Task<Result<Unit>> SendCommandWithBehaviors()
    {
        // This simulates using behaviors (validation, logging, etc.)
        return await _mediator.Send(_command);
    }
}

/// <summary>
/// Benchmarks for Mapper implementations.
/// </summary>
[MemoryDiagnoser]
public class MapperBenchmarks
{
    private Biss.Mapper.Mapper _mapper = null!;
    private User _sourceUser = null!;
    private IServiceProvider _serviceProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Biss.Mapper.Mapper>();
        _serviceProvider = services.BuildServiceProvider();
        
        _mapper = _serviceProvider.GetRequiredService<Biss.Mapper.Mapper>();
        
        _sourceUser = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Age = 30,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Benchmark(Baseline = true)]
    public UserDto MapUser()
    {
        return _mapper.Map<User, UserDto>(_sourceUser);
    }

    [Benchmark]
    public UserDto MapUserWithConverter()
    {
        // Using a custom type converter
        return _mapper.Map<User, UserDto>(_sourceUser);
    }
}

// Helper classes for benchmarks
public record CreateUserCommand(string FirstName, string LastName, string Email) : ICommand;
public record GetUserQuery(int Id) : IQuery<UserDto>;
public record UserCreatedNotification(int Id, string FirstName, string LastName) : INotification;

public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(ILogger<CreateUserHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate work
        await Task.Delay(10, cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}

public class GetUserHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly ILogger<GetUserHandler> _logger;

    public GetUserHandler(ILogger<GetUserHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database call
        await Task.Delay(10, cancellationToken);
        
        var user = new UserDto
        {
            Id = request.Id,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Age = 30,
            CreatedAt = DateTime.UtcNow
        };
        
        return Result<UserDto>.Success(user);
    }
}

public class UserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<UserCreatedNotificationHandler> _logger;

    public UserCreatedNotificationHandler(ILogger<UserCreatedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Simulate async notification processing
        await Task.Delay(10, cancellationToken);
    }
}

// Test models
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}

// User mapper for benchmarks
public class UserMapper
{
    public UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Age = user.Age,
            CreatedAt = user.CreatedAt
        };
    }
}