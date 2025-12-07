using Biss.Mediator.Abstractions;
using Biss.Mediator.Extensions.DependencyInjection;
using Biss.Mapper.Abstractions;
using Biss.Mapper.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biss.Samples.Integration;

/// <summary>
/// Example demonstrating the integration of Biss Mediator and Mapper.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var mediator = host.Services.GetRequiredService<IMediator>();
        var userMapper = host.Services.GetRequiredService<UserMapper>();
        
        Console.WriteLine("=== Integrated Mediator + Mapper Example ===");
        
        // Create a user
        var createUserCommand = new CreateUserCommand("Alice", "Johnson", "alice.johnson@example.com");
        var createResult = await mediator.Send(createUserCommand);
        
        if (createResult.IsSuccess)
        {
            Console.WriteLine("User created successfully!");
            
            // Get the user and map to DTO
            var getUserQuery = new GetUserQuery(1);
            var getResult = await mediator.Send(getUserQuery);
            
            if (getResult.IsSuccess)
            {
                var user = getResult.Value;
                var userDto = userMapper.ToDto(user);
                
                Console.WriteLine($"Retrieved and mapped user:");
                Console.WriteLine($"  ID: {userDto.Id}");
                Console.WriteLine($"  Name: {userDto.FullName}");
                Console.WriteLine($"  Email: {userDto.Email}");
                Console.WriteLine($"  Address: {userDto.AddressLine}");
                Console.WriteLine($"  Created: {userDto.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                Console.WriteLine($"Failed to get user: {getResult.Error.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Failed to create user: {createResult.Error.Message}");
        }
        
        // Publish notification
        var notification = new UserCreatedNotification(1, "Alice", "Johnson");
        await mediator.Publish(notification);
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register Mediator and scan for handlers
                services.AddMediatorFromAssemblyContaining<Program>();
                
                // Register Mappers and scan for mappers
                services.AddMappersFromAssemblyContaining<Program>();
                
                // Register logging
                services.AddLogging(builder => builder.AddConsole());
            });
}

// Commands
public record CreateUserCommand(string FirstName, string LastName, string Email) : ICommand;

// Queries
public record GetUserQuery(int Id) : IQuery<User>;

// Notifications
public record UserCreatedNotification(int Id, string FirstName, string LastName) : INotification;

// Models
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

public class GetUserHandler : IQueryHandler<GetUserQuery, User>
{
    private readonly ILogger<GetUserHandler> _logger;

    public GetUserHandler(ILogger<GetUserHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<User>> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user with ID: {Id}", request.Id);
        
        // Simulate async work
        await Task.Delay(50, cancellationToken);
        
        // Simulate finding user
        if (request.Id == 1)
        {
            var user = new User
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                CreatedAt = DateTime.UtcNow,
                Address = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    Country = "USA"
                }
            };
            
            return Result<User>.Success(user);
        }
        
        return Result<User>.Failure(Error.NotFound("User"));
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

// Mapper
[Mapper]
public partial class UserMapper
{
    public UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            AddressLine = $"{user.Address.Street}, {user.Address.City}, {user.Address.Country}",
            CreatedAt = user.CreatedAt
        };
    }
}
