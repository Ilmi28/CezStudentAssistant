using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.User;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Queries.User;

[TestFixture]
public class GetUserConfigurationQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private IJobRepository _jobRepository = null!;
    private GetUserConfigurationQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _jobRepository = Substitute.For<IJobRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<IJobRepository>().Returns(_jobRepository);
        _sut = new GetUserConfigurationQueryHandler(_unitOfWork);

        var emptyJobsMock = new List<Job>().BuildMockDbSet();
        _jobRepository.Find(Arg.Any<Expression<Func<Job, bool>>>(), Arg.Any<bool>()).Returns(emptyJobsMock);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnConfiguration_WhenUserExistsWithConfiguration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserConfigurationQuery { UserId = userId };

        var user = new Domain.Entities.User
        {
            Id = userId,
            UserName = "testuser",
            CezUser = new CezUser { Token = "token", PrivateToken = "ptoken" },
            Configuration = new UserConfiguration
            {
                Theme = UserTheme.Dark,
                Language = UserLanguage.English
            }
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<Domain.Entities.User, object>>[]>())
            .Returns(user);

        var lastSyncDate = DateTime.UtcNow.AddHours(-1);
        var syncJob = new Job
        {
            UserId = userId,
            JobId = "job1",
            Type = JobType.CezSync,
            Status = JobStatus.Succeeded,
            LastModifiedAt = lastSyncDate
        };
        var jobsMock = new List<Job> { syncJob }.BuildMockDbSet();
        _jobRepository.Find(Arg.Any<Expression<Func<Job, bool>>>(), Arg.Any<bool>()).Returns(jobsMock);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeTrue();
        result.Data.Theme.Should().Be(UserTheme.Dark);
        result.Data.Language.Should().Be(UserLanguage.English);
    }

    [Test]
    public async Task Handle_ShouldReturnDefaults_WhenUserExistsWithoutConfiguration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserConfigurationQuery { UserId = userId };

        var user = new Domain.Entities.User
        {
            Id = userId,
            UserName = "testuser_noconfig",
            CezUser = null,
            Configuration = null
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<Domain.Entities.User, object>>[]>())
            .Returns(user);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeFalse();
        result.Data.Theme.Should().Be(UserTheme.Light);
        result.Data.Language.Should().Be(UserLanguage.Polish);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserConfigurationQuery { UserId = userId };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<Domain.Entities.User, object>>[]>())
            .Returns((Domain.Entities.User?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(UserMessageConsts.UserNotFound);
    }
}
