using CezStudentAssistant.Application.Commands.Cez;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Cez;

[TestFixture]
public class SyncCezCoursesCommandHandlerTests
{
    private IJobScheduler _jobScheduler = null!;
    private IJobService _jobService = null!;
    private SyncCezCoursesCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _jobScheduler = Substitute.For<IJobScheduler>();
        _jobService = Substitute.For<IJobService>();

        _sut = new SyncCezCoursesCommandHandler(_jobScheduler, _jobService);
    }

    [Test]
    public async Task Handle_ShouldCallJobSchedulerAndSaveJob_WhenCommandIsValid()
    {
        var userId = Guid.NewGuid();
        var command = new SyncCezCoursesCommand { UserId = userId };
        
        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued, Type = JobType.CezSync };
        _jobService.CreateJobAsync(userId, JobType.CezSync, Arg.Any<CancellationToken>()).Returns(job);
        _jobScheduler.Enqueue<ICezService>(Arg.Any<Expression<Action<ICezService>>>()).Returns("job-123");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();

        _jobScheduler.Received(1).Enqueue<ICezService>(Arg.Any<Expression<Action<ICezService>>>());
        await _jobService.Received(1).CreateJobAsync(userId, JobType.CezSync, Arg.Any<CancellationToken>());
        await _jobService.Received(1).UpdateJobAsync(job, JobStatus.Enqueued, "job-123", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowException_WhenJobSchedulerThrows()
    {
        var userId = Guid.NewGuid();
        var command = new SyncCezCoursesCommand { UserId = userId };

        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued, Type = JobType.CezSync };
        _jobService.CreateJobAsync(userId, JobType.CezSync, Arg.Any<CancellationToken>()).Returns(job);

        _jobScheduler.When(x => x.Enqueue<ICezService>(Arg.Any<Expression<Action<ICezService>>>()))
            .Do(x => throw new Exception("Hangfire Error"));

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
