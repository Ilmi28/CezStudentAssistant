using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.Cez;
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
using UserEntity = CezStudentAssistant.Domain.Entities.User;

namespace CezStudentAssistant.UnitTests.Application.Queries.Cez;

[TestFixture]
public class GetCezStatusQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private IJobRepository _jobRepository = null!;
    private GetCezStatusQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _jobRepository = Substitute.For<IJobRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<IJobRepository>().Returns(_jobRepository);

        _sut = new GetCezStatusQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnConnectedWithLastSync_WhenUserIsConnectedAndHasSyncJob()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            UserName = "testuser",
            CezUser = new CezUser { Token = "token", PrivateToken = "ptoken" }
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<UserEntity, object>>[]>())
            .Returns(user);

        var syncDate = DateTime.UtcNow.AddHours(-2);
        var job = new Job
        {
            UserId = userId,
            JobId = "job-1",
            Type = JobType.CezSync,
            Status = JobStatus.Succeeded,
            LastModifiedAt = syncDate
        };
        var jobsDbSet = new List<Job> { job }.BuildMockDbSet();
        _jobRepository.Find(Arg.Any<Expression<Func<Job, bool>>>(), Arg.Any<bool>()).Returns(jobsDbSet);

        var query = new GetCezStatusQuery { UserId = userId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsConnected.Should().BeTrue();
        result.Data.LastSyncAt.Should().Be(syncDate);
        result.Data.LastSyncStatus.Should().Be(JobStatus.Succeeded);
    }

    [Test]
    public async Task Handle_ShouldReturnConnectedWithoutSync_WhenUserIsConnectedButHasNoSyncJobs()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            UserName = "testuser",
            CezUser = new CezUser { Token = "token", PrivateToken = "ptoken" }
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<UserEntity, object>>[]>())
            .Returns(user);

        var emptyJobsDbSet = new List<Job>().BuildMockDbSet();
        _jobRepository.Find(Arg.Any<Expression<Func<Job, bool>>>(), Arg.Any<bool>()).Returns(emptyJobsDbSet);

        var query = new GetCezStatusQuery { UserId = userId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsConnected.Should().BeTrue();
        result.Data.LastSyncAt.Should().BeNull();
        result.Data.LastSyncStatus.Should().BeNull();
    }

    [Test]
    public async Task Handle_ShouldReturnNotConnected_WhenUserHasNoCezUser()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            UserName = "testuser",
            CezUser = null
        };

        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<UserEntity, object>>[]>())
            .Returns(user);

        var query = new GetCezStatusQuery { UserId = userId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsConnected.Should().BeFalse();
        result.Data.LastSyncAt.Should().BeNull();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(
            userId,
            Arg.Any<CancellationToken>(),
            true,
            Arg.Any<Expression<Func<UserEntity, object>>[]>())
            .Returns((UserEntity?)null);

        var query = new GetCezStatusQuery { UserId = userId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(UserMessageConsts.UserNotFound);
    }
}
