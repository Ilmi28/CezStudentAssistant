using CezStudentAssistant.Application.CommandHandlers.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Interfaces.Persistence;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace CezStudentAssistant.UnitTests.Application.CommandHandlers.Auth;

public class LoginWithCezCommandHandlerTests
{
    private readonly IValidator<LoginWithCezCommand> _validator;
    private readonly ICezService _cezAuthService;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoginWithCezCommandHandler _sut;

    public LoginWithCezCommandHandlerTests()
    {
        _validator = Substitute.For<IValidator<LoginWithCezCommand>>();
        _cezAuthService = Substitute.For<ICezService>();
        _tokenService = Substitute.For<ITokenService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new LoginWithCezCommandHandler(_validator, _cezAuthService, _tokenService, _currentUserService, _unitOfWork);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task HandleAsync_ShouldCallCezAuthService_WhenDataIsValid()
    {
        var command = new LoginWithCezCommand { UserName = "cez", Password = "pw" };
        var cezUserInfo = new CezUserInfo
        {
            SiteInfo = new CezSiteInfo { ExternalUserId = 1, UserName = "cez" },
            Tokens = new CezTokens { Token = "token", PrivateToken = "private" }
        };
        var userId = Guid.NewGuid();

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _cezAuthService.LoginWithCezAsync(command.UserName, command.Password, Arg.Any<CancellationToken>())
            .Returns(cezUserInfo);
        _cezAuthService.SyncCezUser(cezUserInfo, Arg.Any<CancellationToken>())
            .Returns(userId);
        _tokenService.HandleRefreshToken(userId, Arg.Any<CancellationToken>())
            .Returns("refresh-token");
        _tokenService.GenerateAccessToken(userId).Returns("access-token");

        var result = await _sut.HandleAsync(command);

        result.Success.Should().BeTrue();
        await _cezAuthService.Received(1).LoginWithCezAsync(command.UserName, command.Password, Arg.Any<CancellationToken>());
        await _cezAuthService.Received(1).SyncCezUser(cezUserInfo, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _currentUserService.Received(1).SetSession("access-token", "refresh-token");
    }

    [Test]
    public async Task HandleAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        var command = new LoginWithCezCommand { UserName = null!, Password = null! };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(LoginWithCezCommand.UserName), "UserName is required")
            }));

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<ApiValidationException>();
    }
}
