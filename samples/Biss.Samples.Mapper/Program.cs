using Biss.Mapper.Abstractions;
using Biss.Mapper.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biss.Samples.Mapper;

/// <summary>
/// Example demonstrating the usage of Biss Mapper.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var mapper = host.Services.GetRequiredService<UserMapper>();
        
        // Example 1: Simple mapping
        Console.WriteLine("=== Simple Mapping Example ===");
        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            CreatedAt = DateTime.UtcNow,
            Address = new Address
            {
                Street = "123 Main St",
                City = "New York",
                Country = "USA"
            }
        };
        
        var userDto = mapper.ToDto(user);
        Console.WriteLine($"Mapped User: {userDto.FullName} ({userDto.Email})");
        Console.WriteLine($"Address: {userDto.AddressLine}");
        
        // Example 2: Collection mapping
        Console.WriteLine("\n=== Collection Mapping Example ===");
        var users = new List<User>
        {
            user,
            new User
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                CreatedAt = DateTime.UtcNow,
                Address = new Address
                {
                    Street = "456 Oak Ave",
                    City = "Los Angeles",
                    Country = "USA"
                }
            }
        };
        
        var userDtos = mapper.ToDtoCollection(users);
        foreach (var dto in userDtos)
        {
            Console.WriteLine($"- {dto.FullName} ({dto.Email})");
        }
        
        // Example 3: Reverse mapping
        Console.WriteLine("\n=== Reverse Mapping Example ===");
        var userDto2 = new UserDto
        {
            Id = 3,
            FullName = "Bob Johnson",
            Email = "bob.johnson@example.com",
            AddressLine = "789 Pine St, Seattle, USA",
            CreatedAt = DateTime.UtcNow
        };
        
        var mappedUser = mapper.FromDto(userDto2);
        Console.WriteLine($"Reverse mapped User: {mappedUser.FirstName} {mappedUser.LastName}");
        Console.WriteLine($"Address: {mappedUser.Address.Street}, {mappedUser.Address.City}, {mappedUser.Address.Country}");
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register Mappers and scan for mappers and converters
                services.AddMappersFromAssemblyContaining<Program>();
                
                // Register logging
                services.AddLogging(builder => builder.AddConsole());
            });
}

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

// Mapper
[Mapper]
public partial class UserMapper
{
    // Simple mapping
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
    
    // Collection mapping
    public IEnumerable<UserDto> ToDtoCollection(IEnumerable<User> users)
    {
        return users.Select(ToDto);
    }
    
    // Reverse mapping
    public User FromDto(UserDto dto)
    {
        var nameParts = dto.FullName.Split(' ', 2);
        var addressParts = dto.AddressLine.Split(", ");
        
        return new User
        {
            Id = dto.Id,
            FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
            LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            Email = dto.Email,
            CreatedAt = dto.CreatedAt,
            Address = new Address
            {
                Street = addressParts.Length > 0 ? addressParts[0] : string.Empty,
                City = addressParts.Length > 1 ? addressParts[1] : string.Empty,
                Country = addressParts.Length > 2 ? addressParts[2] : string.Empty
            }
        };
    }
}
