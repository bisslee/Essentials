using Biss.Mediator.Abstractions;
using Biss.Mediator.Extensions.DependencyInjection;
using Biss.Mediator.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace Biss.Samples.Behaviors;

/// <summary>
/// Example demonstrating the usage of Biss Mediator Behaviors.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var mediator = host.Services.GetRequiredService<IMediator>();
        
        Console.WriteLine("=== Mediator Behaviors Example ===");
        
        // Example 1: Command with validation
        Console.WriteLine("\n1. Command with Validation:");
        var createUserCommand = new CreateUserCommand("", "Doe", "invalid-email");
        var commandResult = await mediator.Send(createUserCommand);
        
        if (commandResult.IsSuccess)
        {
            Console.WriteLine("User created successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to create user: {commandResult.Error.Message}");
        }
        
        // Example 2: Valid command
        Console.WriteLine("\n2. Valid Command:");
        var validCommand = new CreateUserCommand("John", "Doe", "john.doe@example.com");
        var validResult = await mediator.Send(validCommand);
        
        if (validResult.IsSuccess)
        {
            Console.WriteLine("User created successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to create user: {validResult.Error.Message}");
        }
        
        // Example 3: Query with caching
        Console.WriteLine("\n3. Query with Caching (first call):");
        var getUserQuery = new GetUserQuery(1);
        var queryResult1 = await mediator.Send(getUserQuery);
        
        if (queryResult1.IsSuccess)
        {
            Console.WriteLine($"User found: {queryResult1.Value.Name} ({queryResult1.Value.Email})");
        }
        
        Console.WriteLine("\n4. Query with Caching (second call - should be cached):");
        var queryResult2 = await mediator.Send(getUserQuery);
        
        if (queryResult2.IsSuccess)
        {
            Console.WriteLine($"User found: {queryResult2.Value.Name} ({queryResult2.Value.Email})");
        }
        
        // Example 4: Notification
        Console.WriteLine("\n5. Notification:");
        var notification = new UserCreatedNotification(1, "John", "Doe");
        await mediator.Publish(notification);
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
                
                // Register all behaviors
                services.AddMediatorBehaviors(options =>
                {
                    options.SlowRequestThreshold = TimeSpan.FromMilliseconds(500);
                    options.DefaultCacheDuration = TimeSpan.FromMinutes(2);
                    options.MaxRetryAttempts = 2;
                });
                
                // Register memory cache for caching behavior
                services.AddMemoryCache();
                
                // Register logging
                services.AddLogging(builder => builder.AddConsole());
            });
}

// Commands with validation
public record CreateUserCommand(
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
    string FirstName,
    
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters")]
    string LastName,
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email) : ICommand;

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
        await Task.Delay(200, cancellationToken);
        
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
        await Task.Delay(150, cancellationToken);
        
        _logger.LogInformation("User created notification processed successfully");
    }
}
