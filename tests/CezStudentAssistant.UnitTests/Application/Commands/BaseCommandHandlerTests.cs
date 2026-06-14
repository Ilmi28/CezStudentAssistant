using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;
using FluentAssertions;

namespace CezStudentAssistant.UnitTests.Application.Commands;

public class BaseCommandHandlerTests
{
    private record TestCommand : ICommand;
    private record TestCommandWithResponse : ICommand<string>;

    private class TestCommandHandler : BaseCommandHandler<TestCommand>
    {
        public bool IsExecuted { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        protected override ApiMessage SuccessMessage => new(this, "Success");
        protected override ApiMessage ErrorMessage => new(this, "Error");

        protected override Task ExecuteAsync(TestCommand command, CancellationToken ct)
        {
            IsExecuted = true;
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.CompletedTask;
        }
    }

    private class TestCommandHandlerWithResponse : BaseCommandHandler<TestCommandWithResponse, string>
    {
        public string Result { get; set; } = "Default";
        public Exception? ExceptionToThrow { get; set; }

        protected override ApiMessage SuccessMessage => new(this, "Success");
        protected override ApiMessage ErrorMessage => new(this, "Error");

        protected override Task<string> ExecuteAsync(TestCommandWithResponse command, CancellationToken ct)
        {
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            return Task.FromResult(Result);
        }
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenExecuteAsyncSucceeds()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Success");
        handler.IsExecuted.Should().BeTrue();
    }

    [Test]
    public async Task Handle_ShouldThrowAppException_WhenExecuteAsyncThrowsGenericException()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand();
        handler.ExceptionToThrow = new Exception("Critical failure");

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<AppException>();
        exception.Which.Message.Should().Be("Error");
        exception.Which.InnerException.Should().BeOfType<Exception>();
        exception.Which.InnerException!.Message.Should().Be("Critical failure");
    }

    [Test]
    public async Task Handle_ShouldNotWrapAppException_WhenExecuteAsyncThrowsAppException()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand();
        var originalAppException = new AppException(new ApiMessage(this, "Original Error"));
        handler.ExceptionToThrow = originalAppException;

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<AppException>();
        exception.Which.Should().Be(originalAppException);
        exception.Which.Message.Should().Be("Original Error");
    }

    [Test]
    public async Task HandleWithResponse_ShouldReturnSuccessResponse_WhenExecuteAsyncSucceeds()
    {
        // Arrange
        var handler = new TestCommandHandlerWithResponse { Result = "TestResult" };
        var command = new TestCommandWithResponse();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SuccessResponse<string>>();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Success");
        result.Data.Should().Be("TestResult");
    }

    [Test]
    public async Task HandleWithResponse_ShouldThrowAppException_WhenExecuteAsyncThrowsGenericException()
    {
        // Arrange
        var handler = new TestCommandHandlerWithResponse();
        var command = new TestCommandWithResponse();
        handler.ExceptionToThrow = new Exception("Critical failure");

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<AppException>();
        exception.Which.Message.Should().Be("Error");
        exception.Which.InnerException!.Message.Should().Be("Critical failure");
    }

    [Test]
    public async Task HandleWithResponse_ShouldNotWrapAppException_WhenExecuteAsyncThrowsAppException()
    {
        // Arrange
        var handler = new TestCommandHandlerWithResponse();
        var command = new TestCommandWithResponse();
        var originalAppException = new AppException(new ApiMessage(this, "Original Error"));
        handler.ExceptionToThrow = originalAppException;

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<AppException>();
        exception.Which.Should().Be(originalAppException);
    }
}
