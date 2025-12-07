using Biss.Mediator.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Biss.Mediator.Tests;

public class MediatorTests
{
    [Fact]
    public async Task Send_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = Mock.Of<ILogger<Mediator>>();
        var mediator = new Mediator(serviceProvider, logger);
        var command = new TestCommand("test");

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Send_WithValidQuery_ShouldReturnResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = Mock.Of<ILogger<Mediator>>();
        var mediator = new Mediator(serviceProvider, logger);
        var query = new TestQuery("test");

        // Act
        var result = await mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    [Fact]
    public async Task Publish_WithValidNotification_ShouldComplete()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = new Mediator(serviceProvider, Mock.Of<ILogger<Mediator>>());
        var notification = new TestNotification("test");

        // Act
        await mediator.Publish(notification);

        // Assert
        // Should complete without throwing
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        services.AddSingleton<ICommandHandler<TestCommand>>(new TestCommandHandler());
        services.AddSingleton<IQueryHandler<TestQuery, string>>(new TestQueryHandler());
        services.AddSingleton<INotificationHandler<TestNotification>>(new TestNotificationHandler());
        
        return services.BuildServiceProvider();
    }
}

// Test implementations
public record TestCommand(string Value) : ICommand;
public record TestQuery(string Value) : IQuery<string>;
public record TestNotification(string Value) : INotification;

public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public Task<Result<Unit>> Handle(TestCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<Unit>.Success(Unit.Value));
    }
}

public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<Result<string>> Handle(TestQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<string>.Success(request.Value));
    }
}

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
