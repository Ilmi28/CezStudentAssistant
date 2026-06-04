using CezStudentAssistant.Application.CommandHandlers;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.CommandHandlers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace CezStudentAssistant.UnitTests.Application.CommandHandlers;

public class BaseCommandHandlerTests
{
    private TestCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new TestCommandHandler();
    }

    [Test]
    public async Task HandleAsync_ShouldReturnSuccessResponse_WhenExecutionIsSuccessful()
    {
        var command = new TestCommand();

        var result = await _sut.HandleAsync(command);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Success");
    }

    [Test]
    public async Task HandleAsync_ShouldThrowAppException_WhenExecutionFails()
    {
        var command = new TestCommand { ShouldFail = true };

        Func<Task> act = () => _sut.HandleAsync(command);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.WithMessage("Error");
        ex.WithInnerExceptionExactly<Exception>().WithMessage("Execution failed");
    }

    public class TestCommand : ICommand 
    {
        public bool ShouldFail { get; set; }
    }

    private class TestCommandHandler : BaseCommandHandler<TestCommand>
    {
        protected override ApiMessage SuccessMessage => new(this, "Success");
        protected override ApiMessage ErrorMessage => new(this, "Error");

        protected override Task ExecuteAsync(TestCommand command, CancellationToken ct)
        {
            if (command.ShouldFail)
                throw new Exception("Execution failed");

            return Task.CompletedTask;
        }
    }
}

public class ValidatableCommandHandlerTests
{
    private IValidator<TestCommandV> _validator = null!;
    private TestValidatableCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = Substitute.For<IValidator<TestCommandV>>();
        _sut = new TestValidatableCommandHandler(_validator);
    }

    [Test]
    public async Task HandleAsync_ShouldReturnSuccessResponse_WhenValidationAndExecutionAreSuccessful()
    {
        var command = new TestCommandV();
        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var result = await _sut.HandleAsync(command);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
    }

    [Test]
    public async Task HandleAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        var command = new TestCommandV();
        var failures = new List<ValidationFailure> { new("Prop", "Error") };
        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<ApiValidationException>()
            .Where(e => e.Message == "Validation failed");
    }

    public class TestCommandV : ICommand { }

    private class TestValidatableCommandHandler(IValidator<TestCommandV> validator) 
        : ValidatableCommandHandler<TestCommandV>(validator)
    {
        protected override ApiMessage SuccessMessage => new(this, "Success");
        protected override ApiMessage ErrorMessage => new(this, "Error");
        protected override ApiMessage ValidationMessage => new(this, "Validation failed");

        protected override Task ExecuteAsync(TestCommandV command, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}

public class ValidatableCommandHandlerWithResponseTests
{
    private IValidator<TestCommandV> _validator = null!;
    private TestValidatableCommandHandlerWithResponse _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = Substitute.For<IValidator<TestCommandV>>();
        _sut = new TestValidatableCommandHandlerWithResponse(_validator);
    }

    [Test]
    public async Task HandleAsync_ShouldReturnSuccessResponse_WhenValidationAndExecutionAreSuccessful()
    {
        var command = new TestCommandV();
        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var result = await _sut.HandleAsync(command);

        result.Should().BeOfType<SuccessResponse<TestResponse>>();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Test]
    public async Task HandleAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        var command = new TestCommandV();
        var failures = new List<ValidationFailure> { new("Prop", "Error") };
        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<ApiValidationException>()
            .Where(e => e.Message == "Validation failed");
    }

    public class TestCommandV : ICommand { }
    public class TestResponse { }

    private class TestValidatableCommandHandlerWithResponse(IValidator<TestCommandV> validator) 
        : ValidatableCommandHandler<TestCommandV, TestResponse>(validator)
    {
        protected override ApiMessage SuccessMessage => new(this, "Success");
        protected override ApiMessage ErrorMessage => new(this, "Error");
        protected override ApiMessage ValidationMessage => new(this, "Validation failed");

        protected override Task<TestResponse> ExecuteAsync(TestCommandV command, CancellationToken ct)
        {
            return Task.FromResult(new TestResponse());
        }
    }
}
