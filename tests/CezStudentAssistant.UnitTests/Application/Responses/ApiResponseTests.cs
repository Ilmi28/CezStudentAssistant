using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.Application.Responses;

[TestFixture]
public class ApiResponseTests
{
    [Test]
    public void ConflictResponse_ShouldMapPropertiesFromMessage()
    {
        // Act
        var response = new ConflictResponse("Username already exists.");

        // Assert
        response.Message.Should().Be("Username already exists.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Test]
    public void NotFoundResponse_ShouldMapPropertiesFromMessage()
    {
        // Act
        var response = new NotFoundResponse("Course not found.");

        // Assert
        response.Message.Should().Be("Course not found.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public void BadRequestResponse_ShouldMapPropertiesFromMessage()
    {
        // Act
        var response = new BadRequestResponse("Invalid parameter value.");

        // Assert
        response.Message.Should().Be("Invalid parameter value.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Test]
    public void BadGatewayResponse_ShouldMapPropertiesFromMessage()
    {
        // Act
        var response = new BadGatewayResponse("External API timeout.");

        // Assert
        response.Message.Should().Be("External API timeout.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadGateway);
    }

    [Test]
    public void ForbiddenResponse_ShouldMapPropertiesFromMessage()
    {
        // Act
        var response = new ForbiddenResponse("Admin access required.");

        // Assert
        response.Message.Should().Be("Admin access required.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Test]
    public void UnauthorizedResponse_ShouldMapPropertiesFromMessage()
    {
        // Act
        var response = new UnauthorizedResponse("Session expired.");

        // Assert
        response.Message.Should().Be("Session expired.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public void ServerErrorResponse_ShouldMapPropertiesFromMessage_WhenMessageProvided()
    {
        // Act
        var response = new ServerErrorResponse("Database connection failure.");

        // Assert
        response.Message.Should().Be("Database connection failure.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }

    [Test]
    public void ServerErrorResponse_ShouldUseDefaultValues_WhenNoMessageProvided()
    {
        // Act
        var response = new ServerErrorResponse();

        // Assert
        response.Message.Should().Be("An unexpected error occurred while processing your request. Please try again later.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }

    [Test]
    public void SuccessResponse_ShouldMapPropertiesFromMessage()
    {
        // Act
        var response = new SuccessResponse("User registered successfully.");

        // Assert
        response.Message.Should().Be("User registered successfully.");
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Test]
    public void SuccessResponseOfT_ShouldMapPropertiesFromMessageAndContainData()
    {
        // Arrange
        var testData = "SomeData";

        // Act
        var response = new SuccessResponse<string>("User details fetched.", testData);

        // Assert
        response.Message.Should().Be("User details fetched.");
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Data.Should().Be(testData);
    }

    [Test]
    public void ValidationResponse_ShouldMapPropertiesFromMessageAndContainErrors()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError { PropertyName = "Username", ErrorMessage = "Username is required." }
        };

        // Act
        var response = new ValidationResponse("Validation failed.", errors);

        // Assert
        response.Message.Should().Be("Validation failed.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        response.Errors.Should().ContainSingle().Which.PropertyName.Should().Be("Username");
    }
}
