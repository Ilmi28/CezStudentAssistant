using CezStudentAssistant.Application.CommandHandlers.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.Interfaces.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace CezStudentAssistant.UnitTests.Application.CommandHandlers.Auth;

public class LoginWithCezCommandHandlerTests
{
    private readonly IValidator<LoginWithCezCommand> _validator;
    private readonly ICezAuthService _cezAuthService;
    private readonly LoginWithCezCommandHandler _sut;

    public LoginWithCezCommandHandlerTests()
    {
        _validator = Substitute.For<IValidator<LoginWithCezCommand>>();
        _cezAuthService = Substitute.For<ICezAuthService>();
        _sut = new LoginWithCezCommandHandler(_validator, _cezAuthService);
    }

    [Test]
    public async Task HandleAsync_ShouldCallCezAuthService_WhenDataIsValid()
    {
        // Arrange
        var command = new LoginWithCezCommand { UserName = "cez", Password = "pw" };
        var expectedResponse = new CezLoginResponse { Success = true };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _cezAuthService.LoginWithCezAsync(command.UserName, command.Password, Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Data.Should().BeEquivalentTo(expectedResponse);
        await _cezAuthService.Received(1).LoginWithCezAsync(command.UserName, command.Password, Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
