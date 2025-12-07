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

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var value = "test";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.NotFound("User");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        // Arrange
        var value = "test";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange
        var error = Error.Validation("Name", "Required");

        // Act
        Result<string> result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}

public class ErrorTests
{
    [Fact]
    public void NotFound_ShouldCreateCorrectError()
    {
        // Act
        var error = Error.NotFound("User");

        // Assert
        error.Code.Should().Be("NOT_FOUND");
        error.Message.Should().Be("Resource 'User' not found");
    }

    [Fact]
    public void Validation_ShouldCreateCorrectError()
    {
        // Act
        var error = Error.Validation("Name", "Required");

        // Assert
        error.Code.Should().Be("VALIDATION");
        error.Message.Should().Be("Field 'Name': Required");
    }

    [Fact]
    public void Unauthorized_ShouldCreateCorrectError()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        error.Code.Should().Be("UNAUTHORIZED");
        error.Message.Should().Be("Access denied");
    }
}

public class UnitTests
{
    [Fact]
    public void Unit_ShouldBeEqual()
    {
        // Arrange
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;

        // Act & Assert
        unit1.Equals(unit2).Should().BeTrue();
        unit1.GetHashCode().Should().Be(unit2.GetHashCode());
    }

    [Fact]
    public void Unit_ToString_ShouldReturnUnit()
    {
        // Arrange
        var unit = Unit.Value;

        // Act
        var result = unit.ToString();

        // Assert
        result.Should().Be("Unit");
    }
}
