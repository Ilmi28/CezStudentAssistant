using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Application.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Application.Services;

public class CezAuthServiceTests
{
    private ICezApiClient _cezApiClient = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private ICezUserRepository _cezUserRepository = null!;
    private CezAuthService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezApiClient = Substitute.For<ICezApiClient>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _cezUserRepository = Substitute.For<ICezUserRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<ICezUserRepository>().Returns(_cezUserRepository);

        _sut = new CezAuthService(_cezApiClient, _unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var userName = "cezUser";
        var password = "password";
        var messageSource = new object();

        _cezApiClient.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = true, Data = new CezTokens { Token = "t", PrivateToken = "pt" } });

        _cezApiClient.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse 
            { 
                Success = true, 
                Data = new CezSiteInfo { UserName = userName, FullName = "Full Name", ExternalUserId = 123 } 
            });

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.LoginWithCezAsync(userName, password, messageSource);

        // Assert
        result.Success.Should().BeTrue();
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.UserName == userName), Arg.Any<CancellationToken>());
        await _cezUserRepository.Received(1).AddAsync(Arg.Is<CezUser>(cu => cu.FullName == "Full Name"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldUpdateExistingUser_WhenUserExists()
    {
        // Arrange
        var userName = "cezUser";
        var userId = Guid.NewGuid();
        var user = new User { UserName = userName };
        user.GetType().GetProperty("Id")?.SetValue(user, userId);

        var existingCezUser = new CezUser 
        { 
            UserId = userId, 
            Token = "old", 
            FullName = "Name", 
            PrivateToken = "pt", 
            ExternalUserId = 1,
            User = user
        };

        _cezApiClient.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = true, Data = new CezTokens { Token = "new_t", PrivateToken = "new_pt" } });

        _cezApiClient.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse
            {
                Success = true,
                Data = new CezSiteInfo { UserName = userName }
            });

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingCezUser);

        // Act
        await _sut.LoginWithCezAsync(userName, "pw", new object());

        // Assert
        existingCezUser.Token.Should().Be("new_t");
        await _cezUserRepository.Received(1).UpdateAsync(existingCezUser, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
