using CezStudentAssistant.Application.Decorators;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CezStudentAssistant.UnitTests.Application.Decorators;

public class LoggingDecoratorTests
{
    #region CommandHandlerDecorator (No Response)
    private ICommandHandler<TestCommand> _innerCommandHandler = null!;
    private ILogger<LoggingDecorator.CommandHandlerDecorator<TestCommand>> _commandLogger = null!;
    private LoggingDecorator.CommandHandlerDecorator<TestCommand> _commandSut = null!;

    [SetUp]
    public void SetUpCommand()
    {
        _innerCommandHandler = Substitute.For<ICommandHandler<TestCommand>>();
        _commandLogger = Substitute.For<ILogger<LoggingDecorator.CommandHandlerDecorator<TestCommand>>>();
        _commandSut = new LoggingDecorator.CommandHandlerDecorator<TestCommand>(_innerCommandHandler, _commandLogger);
    }

    [Test]
    public async Task CommandHandleAsync_ShouldLogStartAndEnd_WhenSuccessful()
    {
        var command = new TestCommand();
        var response = new SuccessResponse();
        _innerCommandHandler.HandleAsync(command, Arg.Any<CancellationToken>()).Returns(response);

        var result = await _commandSut.HandleAsync(command);

        result.Should().Be(response);
        VerifyLog(_commandLogger, LogLevel.Information, "START");
        VerifyLog(_commandLogger, LogLevel.Information, "END");
    }

    [Test]
    public async Task CommandHandleAsync_ShouldLogError_WhenExceptionOccurs()
    {
        var command = new TestCommand();
        var exception = new Exception("Test error");
        _innerCommandHandler.HandleAsync(command, Arg.Any<CancellationToken>()).Throws(exception);

        Func<Task> act = () => _commandSut.HandleAsync(command);

        await act.Should().ThrowAsync<Exception>().WithMessage("Test error");
        VerifyLog(_commandLogger, LogLevel.Error, "ERROR", exception);
    }
    #endregion

    #region CommandHandlerDecorator (With Response)
    private ICommandHandler<TestCommandWithResponse, TestResponse> _innerCommandHandlerWithResponse = null!;
    private ILogger<LoggingDecorator.CommandHandlerDecorator<TestCommandWithResponse, TestResponse>> _commandWithResponseLogger = null!;
    private LoggingDecorator.CommandHandlerDecorator<TestCommandWithResponse, TestResponse> _commandWithResponseSut = null!;

    [SetUp]
    public void SetUpCommandWithResponse()
    {
        _innerCommandHandlerWithResponse = Substitute.For<ICommandHandler<TestCommandWithResponse, TestResponse>>();
        _commandWithResponseLogger = Substitute.For<ILogger<LoggingDecorator.CommandHandlerDecorator<TestCommandWithResponse, TestResponse>>>();
        _commandWithResponseSut = new LoggingDecorator.CommandHandlerDecorator<TestCommandWithResponse, TestResponse>(_innerCommandHandlerWithResponse, _commandWithResponseLogger);
    }

    [Test]
    public async Task CommandWithResponseHandleAsync_ShouldLogStartAndEnd_WhenSuccessful()
    {
        var command = new TestCommandWithResponse();
        var response = new ApiResponse<TestResponse> { Success = true, Data = new TestResponse() };
        _innerCommandHandlerWithResponse.HandleAsync(command, Arg.Any<CancellationToken>()).Returns(response);

        var result = await _commandWithResponseSut.HandleAsync(command);

        result.Should().Be(response);
        VerifyLog(_commandWithResponseLogger, LogLevel.Information, "START");
        VerifyLog(_commandWithResponseLogger, LogLevel.Information, "END");
    }

    [Test]
    public async Task CommandWithResponseHandleAsync_ShouldLogError_WhenExceptionOccurs()
    {
        var command = new TestCommandWithResponse();
        var exception = new Exception("Test error");
        _innerCommandHandlerWithResponse.HandleAsync(command, Arg.Any<CancellationToken>()).Throws(exception);

        Func<Task> act = () => _commandWithResponseSut.HandleAsync(command);

        await act.Should().ThrowAsync<Exception>().WithMessage("Test error");
        VerifyLog(_commandWithResponseLogger, LogLevel.Error, "ERROR", exception);
    }
    #endregion

    #region QueryHandlerDecorator
    private IQueryHandler<TestQuery, TestResponse> _innerQueryHandler = null!;
    private ILogger<LoggingDecorator.QueryHandlerDecorator<TestQuery, TestResponse>> _queryLogger = null!;
    private LoggingDecorator.QueryHandlerDecorator<TestQuery, TestResponse> _querySut = null!;

    [SetUp]
    public void SetUpQuery()
    {
        _innerQueryHandler = Substitute.For<IQueryHandler<TestQuery, TestResponse>>();
        _queryLogger = Substitute.For<ILogger<LoggingDecorator.QueryHandlerDecorator<TestQuery, TestResponse>>>();
        _querySut = new LoggingDecorator.QueryHandlerDecorator<TestQuery, TestResponse>(_innerQueryHandler, _queryLogger);
    }

    [Test]
    public async Task QueryHandleAsync_ShouldLogStartAndEnd_WhenSuccessful()
    {
        var query = new TestQuery();
        var response = new ApiResponse<TestResponse> { Success = true, Data = new TestResponse() };
        _innerQueryHandler.HandleAsync(query, Arg.Any<CancellationToken>()).Returns(response);

        var result = await _querySut.HandleAsync(query);

        result.Should().Be(response);
        VerifyLog(_queryLogger, LogLevel.Information, "START");
        VerifyLog(_queryLogger, LogLevel.Information, "END");
    }

    [Test]
    public async Task QueryHandleAsync_ShouldLogError_WhenExceptionOccurs()
    {
        var query = new TestQuery();
        var exception = new Exception("Test error");
        _innerQueryHandler.HandleAsync(query, Arg.Any<CancellationToken>()).Throws(exception);

        Func<Task> act = () => _querySut.HandleAsync(query);

        await act.Should().ThrowAsync<Exception>().WithMessage("Test error");
        VerifyLog(_queryLogger, LogLevel.Error, "ERROR", exception);
    }
    #endregion

    private void VerifyLog<T>(ILogger<T> logger, LogLevel level, string messagePart, Exception? exception = null)
    {
        logger.Received(1).Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains(messagePart)),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    public class TestCommand : ICommand { }
    public class TestCommandWithResponse : ICommand { }
    public class TestQuery : IQuery { }
    public class TestResponse { }
}
