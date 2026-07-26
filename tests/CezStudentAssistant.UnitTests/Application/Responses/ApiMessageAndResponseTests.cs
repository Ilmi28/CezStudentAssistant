using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.Application.Responses;

[TestFixture]
public class ApiMessageAndResponseTests
{
    private class DummySourceClass { }

    [TestCase("Username already exists.", "USERNAME_ALREADY_EXISTS")]
    [TestCase("Email already exists!", "EMAIL_ALREADY_EXISTS")]
    [TestCase("Some-random_error   message.", "SOME_RANDOM_ERROR_MESSAGE")]
    [TestCase("", "ERROR")]
    [TestCase("    ", "ERROR")]
    public void ApiMessage_ShouldSanitizeMessageToUpperCaseSnakeCaseCode_WhenNoCodeIsProvided(string messageText, string expectedCode)
    {
        // Arrange & Act
        var message = new ApiMessage(new DummySourceClass(), messageText);

        // Assert
        message.Code.Should().Be(expectedCode);
    }

    [Test]
    public void ApiMessage_ShouldUseExplicitCode_WhenProvided()
    {
        // Arrange & Act
        var message = new ApiMessage(new DummySourceClass(), "Custom message", "EXPLICIT_CUSTOM_CODE");

        // Assert
        message.Code.Should().Be("EXPLICIT_CUSTOM_CODE");
        message.Message.Should().Be("Custom message");
    }

    [Test]
    public void ConflictResponse_ShouldMapPropertiesFromMessage()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "Username already exists.");

        // Act
        var response = new ConflictResponse(message);

        // Assert
        response.Message.Should().Be("Username already exists.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Test]
    public void NotFoundResponse_ShouldMapPropertiesFromMessage()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "Course not found.");

        // Act
        var response = new NotFoundResponse(message);

        // Assert
        response.Message.Should().Be("Course not found.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public void BadRequestResponse_ShouldMapPropertiesFromMessage()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "Invalid parameter value.");

        // Act
        var response = new BadRequestResponse(message);

        // Assert
        response.Message.Should().Be("Invalid parameter value.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Test]
    public void BadGatewayResponse_ShouldMapPropertiesFromMessage()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "External API timeout.");

        // Act
        var response = new BadGatewayResponse(message);

        // Assert
        response.Message.Should().Be("External API timeout.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadGateway);
    }

    [Test]
    public void ForbiddenResponse_ShouldMapPropertiesFromMessage()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "Admin access required.");

        // Act
        var response = new ForbiddenResponse(message);

        // Assert
        response.Message.Should().Be("Admin access required.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Test]
    public void UnauthorizedResponse_ShouldMapPropertiesFromMessage()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "Session expired.");

        // Act
        var response = new UnauthorizedResponse(message);

        // Assert
        response.Message.Should().Be("Session expired.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public void ServerErrorResponse_ShouldMapPropertiesFromMessage_WhenMessageProvided()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "Database connection failure.");

        // Act
        var response = new ServerErrorResponse(message);

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
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "User registered successfully.");

        // Act
        var response = new SuccessResponse(message);

        // Assert
        response.Message.Should().Be("User registered successfully.");
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Test]
    public void SuccessResponseOfT_ShouldMapPropertiesFromMessageAndContainData()
    {
        // Arrange
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "User details fetched.");
        var testData = "SomeData";

        // Act
        var response = new SuccessResponse<string>(message, testData);

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
        var source = new DummySourceClass();
        var message = new ApiMessage(source, "Validation failed.");
        var errors = new[]
        {
            new ValidationError { PropertyName = "Username", ErrorMessage = "Username is required." }
        };

        // Act
        var response = new ValidationResponse(message, errors);

        // Assert
        response.Message.Should().Be("Validation failed.");
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        response.Errors.Should().ContainSingle().Which.PropertyName.Should().Be("Username");
    }
}
