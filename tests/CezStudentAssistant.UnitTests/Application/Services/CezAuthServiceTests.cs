using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
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
    private CezService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezApiClient = Substitute.For<ICezApiClient>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _cezUserRepository = Substitute.For<ICezUserRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<ICezUserRepository>().Returns(_cezUserRepository);

        _sut = new CezService(_cezApiClient, _unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldReturnUserInfo_WhenCezApiReturnsValidData()
    {
        var userName = "cezUser";
        var password = "password";
        var tokens = new CezTokens { Token = "t", PrivateToken = "pt" };
        var siteInfo = new CezSiteInfo { UserName = userName, FullName = "Full Name", ExternalUserId = 123 };

        _cezApiClient.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = true, Data = tokens });

        _cezApiClient.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse 
            { 
                Success = true, 
                Data = siteInfo
            });

        var result = await _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        result.SiteInfo.Should().BeEquivalentTo(siteInfo);
        result.Tokens.Should().BeEquivalentTo(tokens);
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldThrowBadRequest_WhenLoginFails()
    {
        var userName = "cezUser";
        var password = "password";

        _cezApiClient.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = false, Message = "Invalid credentials" });

        Func<Task> act = () => _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldThrowBadRequest_WhenSiteInfoFails()
    {
        var userName = "cezUser";
        var password = "password";
        var tokens = new CezTokens { Token = "t", PrivateToken = "pt" };

        _cezApiClient.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = true, Data = tokens });

        _cezApiClient.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse { Success = false, Message = "Bad response" });

        Func<Task> act = () => _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task SyncCezUser_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        var userName = "cezUser";
        var cezUserInfo = new CezUserInfo
        {
            SiteInfo = new CezSiteInfo { UserName = userName, FullName = "Full Name", ExternalUserId = 123 },
            Tokens = new CezTokens { Token = "new_t", PrivateToken = "new_pt" }
        };

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.SyncCezUser(cezUserInfo, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.UserName == userName), Arg.Any<CancellationToken>());
        await _cezUserRepository.Received(1).AddAsync(Arg.Is<CezUser>(cu => cu.FullName == "Full Name"), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SyncCezUser_ShouldThrowNotFound_WhenCezUserIsMissing()
    {
        var userName = "cezUser";
        var userId = Guid.NewGuid();
        var user = new User { UserName = userName };
        user.GetType().GetProperty("Id")?.SetValue(user, userId);

        var cezUserInfo = new CezUserInfo
        {
            SiteInfo = new CezSiteInfo { UserName = userName },
            Tokens = new CezTokens { Token = "new_t", PrivateToken = "new_pt" }
        };

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CezUser?)null);

        Func<Task> act = () => _sut.SyncCezUser(cezUserInfo, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task SyncCezUser_ShouldUpdateExistingUser_WhenUserExists()
    {
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

        var cezUserInfo = new CezUserInfo
        {
            SiteInfo = new CezSiteInfo { UserName = userName },
            Tokens = new CezTokens { Token = "new_t", PrivateToken = "new_pt" }
        };

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingCezUser);

        var result = await _sut.SyncCezUser(cezUserInfo, CancellationToken.None);

        result.Should().Be(userId);
        existingCezUser.Token.Should().Be("new_t");
        await _cezUserRepository.Received(1).UpdateAsync(existingCezUser, Arg.Any<CancellationToken>());
    }
}
