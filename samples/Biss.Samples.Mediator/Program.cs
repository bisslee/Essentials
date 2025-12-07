using Biss.Mediator.Abstractions;
using Biss.Mediator.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biss.Samples.Mediator;

/// <summary>
/// Example demonstrating the usage of Biss Mediator.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var mediator = host.Services.GetRequiredService<IMediator>();
        
        // Example 1: Command without return value
        Console.WriteLine("=== Command Example ===");
        var createUserCommand = new CreateUserCommand("John", "Doe", "john.doe@example.com");
        var commandResult = await mediator.Send(createUserCommand);
        
        if (commandResult.IsSuccess)
        {
            Console.WriteLine("User created successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to create user: {commandResult.Error.Message}");
        }
        
        // Example 2: Query with return value
        Console.WriteLine("\n=== Query Example ===");
        var getUserQuery = new GetUserQuery(1);
        var queryResult = await mediator.Send(getUserQuery);
        
        if (queryResult.IsSuccess)
        {
            Console.WriteLine($"User found: {queryResult.Value.Name} ({queryResult.Value.Email})");
        }
        else
        {
            Console.WriteLine($"Failed to get user: {queryResult.Error.Message}");
        }
        
        // Example 3: Notification (fire-and-forget)
        Console.WriteLine("\n=== Notification Example ===");
        var userCreatedNotification = new UserCreatedNotification(1, "John", "Doe");
        await mediator.Publish(userCreatedNotification);
        Console.WriteLine("User created notification sent!");
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register Mediator and scan for handlers
                services.AddMediatorFromAssemblyContaining<Program>();
                
                // Register logging
                services.AddLogging(builder => builder.AddConsole());
            });
}

// Commands
public record CreateUserCommand(string FirstName, string LastName, string Email) : ICommand;

// Queries
public record GetUserQuery(int Id) : IQuery<UserDto>;

// Notifications
public record UserCreatedNotification(int Id, string FirstName, string LastName) : INotification;

// DTOs
public record UserDto(int Id, string Name, string Email);

// Handlers
public class CreateUserHandler : ICommandHandler<CreateUserCommand>
{
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(ILogger<CreateUserHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user: {FirstName} {LastName} ({Email})", 
            request.FirstName, request.LastName, request.Email);
        
        // Simulate async work
        await Task.Delay(100, cancellationToken);
        
        // Simulate success
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
        _logger.LogInformation("Getting user with ID: {Id}", request.Id);
        
        // Simulate async work
        await Task.Delay(50, cancellationToken);
        
        // Simulate finding user
        if (request.Id == 1)
        {
            var user = new UserDto(1, "John Doe", "john.doe@example.com");
            return Result<UserDto>.Success(user);
        }
        
        return Result<UserDto>.Failure(Error.NotFound("User"));
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
        _logger.LogInformation("Processing user created notification for user {Id}: {FirstName} {LastName}", 
            notification.Id, notification.FirstName, notification.LastName);
        
        // Simulate async work (e.g., sending email, updating cache, etc.)
        await Task.Delay(200, cancellationToken);
        
        _logger.LogInformation("User created notification processed successfully");
    }
}
