using CezStudentAssistant.Application.Commands.UserConfiguration;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.UserConfiguration;

[TestFixture]
public class UpdateUserConfigurationCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private IGenericRepository<Domain.Entities.UserConfiguration> _configRepository = null!;
    private UpdateUserConfigurationCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _configRepository = Substitute.For<IGenericRepository<Domain.Entities.UserConfiguration>>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<IGenericRepository<Domain.Entities.UserConfiguration>>().Returns(_configRepository);

        _sut = new UpdateUserConfigurationCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCreateConfigurationWithDefaults_WhenUserHasNoConfigurationAndNullValuesProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand
        {
            UserId = userId,
            Theme = null,
            Language = null
        };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Configuration = null,
            CezUser = null
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.UpdateUserConfigurationSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Theme.Should().Be(UserTheme.Light);
        result.Data.Language.Should().Be(UserLanguage.Polish);
        result.Data.IsCezConnected.Should().BeFalse();

        await _configRepository.Received(1).AddAsync(Arg.Is<Domain.Entities.UserConfiguration>(c =>
            c.UserId == userId && c.Theme == UserTheme.Light && c.Language == UserLanguage.Polish), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldCreateConfigurationWithCustomValues_WhenUserHasNoConfigurationAndValuesProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand
        {
            UserId = userId,
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Configuration = null,
            CezUser = null
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Theme.Should().Be(UserTheme.Dark);
        result.Data.Language.Should().Be(UserLanguage.English);
        result.Data.IsCezConnected.Should().BeFalse();

        await _configRepository.Received(1).AddAsync(Arg.Is<Domain.Entities.UserConfiguration>(c =>
            c.UserId == userId && c.Theme == UserTheme.Dark && c.Language == UserLanguage.English), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldUpdateExistingConfiguration_WhenUserAlreadyHasConfiguration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand
        {
            UserId = userId,
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        var existingConfig = new Domain.Entities.UserConfiguration
        {
            UserId = userId,
            Theme = UserTheme.Light,
            Language = UserLanguage.Polish
        };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Configuration = existingConfig,
            CezUser = new CezUser { Token = "token", PrivateToken = "ptoken" }
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Theme.Should().Be(UserTheme.Dark);
        result.Data.Language.Should().Be(UserLanguage.English);
        result.Data.IsCezConnected.Should().BeTrue();

        existingConfig.Theme.Should().Be(UserTheme.Dark);
        existingConfig.Language.Should().Be(UserLanguage.English);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldUpdateOnlyTheme_WhenOnlyThemeIsProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand
        {
            UserId = userId,
            Theme = UserTheme.Dark,
            Language = null
        };

        var existingConfig = new Domain.Entities.UserConfiguration
        {
            UserId = userId,
            Theme = UserTheme.Light,
            Language = UserLanguage.Polish
        };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Configuration = existingConfig
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingConfig.Theme.Should().Be(UserTheme.Dark);
        existingConfig.Language.Should().Be(UserLanguage.Polish);
    }

    [Test]
    public async Task Handle_ShouldUpdateOnlyLanguage_WhenOnlyLanguageIsProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand
        {
            UserId = userId,
            Theme = null,
            Language = UserLanguage.English
        };

        var existingConfig = new Domain.Entities.UserConfiguration
        {
            UserId = userId,
            Theme = UserTheme.Dark,
            Language = UserLanguage.Polish
        };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Configuration = existingConfig
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingConfig.Theme.Should().Be(UserTheme.Dark);
        existingConfig.Language.Should().Be(UserLanguage.English);
    }

    [Test]
    public async Task Handle_ShouldLeaveConfigUnchanged_WhenExistingConfigAndNullValuesProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand
        {
            UserId = userId,
            Theme = null,
            Language = null
        };

        var existingConfig = new Domain.Entities.UserConfiguration
        {
            UserId = userId,
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Configuration = existingConfig
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        existingConfig.Theme.Should().Be(UserTheme.Dark);
        existingConfig.Language.Should().Be(UserLanguage.English);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldReturnIsCezConnectedTrue_WhenCezUserIsNotNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand { UserId = userId, Theme = UserTheme.Dark };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            CezUser = new CezUser { Token = "token", PrivateToken = "ptoken" },
            Configuration = new Domain.Entities.UserConfiguration { Theme = UserTheme.Light, Language = UserLanguage.Polish }
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeTrue();
    }

    [Test]
    public async Task Handle_ShouldReturnIsCezConnectedFalse_WhenCezUserIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand { UserId = userId, Theme = UserTheme.Dark };

        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            CezUser = null,
            Configuration = new Domain.Entities.UserConfiguration { Theme = UserTheme.Light, Language = UserLanguage.Polish }
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeFalse();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserConfigurationCommand
        {
            UserId = userId,
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<User, object>>[]>())
            .Returns((User?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(UserMessageConsts.UserNotFound);
    }
}
