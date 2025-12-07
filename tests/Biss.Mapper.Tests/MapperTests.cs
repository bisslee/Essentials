using Biss.Mapper.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Biss.Mapper.Tests;

public class MapperTests
{
    [Fact]
    public void Map_WithSimpleTypes_ShouldMapProperties()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mapper = new Mapper(serviceProvider);
        var source = new TestSource { Id = 1, Name = "Test" };

        // Act
        var result = mapper.Map<TestSource, TestDestination>(source);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
    }

    [Fact]
    public void Map_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mapper = new Mapper(serviceProvider);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.Map<TestSource, TestDestination>(null!));
    }

    [Fact]
    public void Map_WithCustomConverter_ShouldUseConverter()
    {
        // Arrange
        var serviceProvider = CreateServiceProviderWithConverter();
        var mapper = new Mapper(serviceProvider);
        var source = new TestSource { Id = 1, Name = "Test" };

        // Act
        var result = mapper.Map<TestSource, TestDestination>(source);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be($"Converted: {source.Name}");
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        return services.BuildServiceProvider();
    }

    private static IServiceProvider CreateServiceProviderWithConverter()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITypeConverter<TestSource, TestDestination>, TestConverter>();
        return services.BuildServiceProvider();
    }
}

// Test models
public class TestSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestDestination
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestConverter : ITypeConverter<TestSource, TestDestination>
{
    public TestDestination Convert(TestSource source, IMappingContext context)
    {
        return new TestDestination
        {
            Id = source.Id,
            Name = $"Converted: {source.Name}"
        };
    }
}
