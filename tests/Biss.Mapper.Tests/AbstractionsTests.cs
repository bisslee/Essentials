using Biss.Mapper.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Biss.Mapper.Tests;

public class MapperAttributeTests
{
    [Fact]
    public void MapperAttribute_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var attribute = new MapperAttribute();

        // Assert
        attribute.IgnoreNullValues.Should().BeFalse();
        attribute.IgnoreNullCollections.Should().BeFalse();
        attribute.UseDeepCopy.Should().BeFalse();
    }

    [Fact]
    public void MapperAttribute_ShouldAllowSettingValues()
    {
        // Arrange & Act
        var attribute = new MapperAttribute
        {
            IgnoreNullValues = true,
            IgnoreNullCollections = true,
            UseDeepCopy = true
        };

        // Assert
        attribute.IgnoreNullValues.Should().BeTrue();
        attribute.IgnoreNullCollections.Should().BeTrue();
        attribute.UseDeepCopy.Should().BeTrue();
    }
}

public class MapPropertyAttributeTests
{
    [Fact]
    public void MapPropertyAttribute_ShouldSetSourceProperty()
    {
        // Arrange & Act
        var attribute = new MapPropertyAttribute("SourceProp");

        // Assert
        attribute.SourceProperty.Should().Be("SourceProp");
        attribute.TargetProperty.Should().BeNull();
        attribute.Ignore.Should().BeFalse();
        attribute.Converter.Should().BeNull();
    }

    [Fact]
    public void MapPropertyAttribute_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var attribute = new MapPropertyAttribute("SourceProp")
        {
            TargetProperty = "TargetProp",
            Ignore = true,
            Converter = "CustomConverter"
        };

        // Assert
        attribute.SourceProperty.Should().Be("SourceProp");
        attribute.TargetProperty.Should().Be("TargetProp");
        attribute.Ignore.Should().BeTrue();
        attribute.Converter.Should().Be("CustomConverter");
    }
}

public class MapConditionAttributeTests
{
    [Fact]
    public void MapConditionAttribute_ShouldSetCondition()
    {
        // Arrange & Act
        var attribute = new MapConditionAttribute("source.Id > 0");

        // Assert
        attribute.Condition.Should().Be("source.Id > 0");
    }
}

public class MappingContextTests
{
    [Fact]
    public void MappingContext_ShouldProvideServiceProvider()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new TestMappingContext(serviceProvider);

        // Act & Assert
        context.ServiceProvider.Should().Be(serviceProvider);
        context.Items.Should().NotBeNull();
    }

    [Fact]
    public void MappingContext_Items_ShouldBeMutable()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var context = new TestMappingContext(serviceProvider);

        // Act
        context.Items["test"] = "value";

        // Assert
        context.Items["test"].Should().Be("value");
    }
}

// Test implementation
internal class TestMappingContext : IMappingContext
{
    public IServiceProvider ServiceProvider { get; }
    public IDictionary<string, object> Items { get; }

    public TestMappingContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Items = new Dictionary<string, object>();
    }
}
