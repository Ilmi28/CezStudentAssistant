using CezStudentAssistant.Application.CommandHandlers.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Interfaces.Persistence;
using FluentAssertions;
using NSubstitute;

namespace CezStudentAssistant.UnitTests.Application.CommandHandlers.Auth;

public class LoginWithCezCommandHandlerTests
{
    private ICezService _cezAuthService = null!;
    private ITokenService _tokenService = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private LoginWithCezCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezAuthService = Substitute.For<ICezService>();
        _tokenService = Substitute.For<ITokenService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new LoginWithCezCommandHandler(_cezAuthService, _tokenService, _currentUserService, _unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCallCezAuthService_WhenDataIsValid()
    {
        var command = new LoginWithCezCommand { UserName = "cez", Password = "pw" };
        var userId = Guid.NewGuid();

        _cezAuthService.LoginWithCezAsync(command.UserName, command.Password, Arg.Any<CancellationToken>())
            .Returns(userId);
        _tokenService.HandleRefreshToken(userId, Arg.Any<CancellationToken>())
            .Returns("refresh-token");
        _tokenService.GenerateAccessToken(userId).Returns("access-token");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _cezAuthService.Received(1).LoginWithCezAsync(command.UserName, command.Password, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _currentUserService.Received(1).SetSession("access-token", "refresh-token");
    }
}
