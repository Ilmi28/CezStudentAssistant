using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Services;
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

namespace CezStudentAssistant.UnitTests.Application.Services;

[TestFixture]
public class JobServiceTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IJobNotificationService _notificationService = null!;
    private IJobRepository _jobRepository = null!;
    private JobService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _notificationService = Substitute.For<IJobNotificationService>();
        _jobRepository = Substitute.For<IJobRepository>();

        _unitOfWork.Repository<IJobRepository>().Returns(_jobRepository);
        _sut = new JobService(_unitOfWork, _notificationService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task GetLatestJobAsync_ShouldReturnJob_WhenExists()
    {
        var userId = Guid.NewGuid();
        var job = new Job
        {
            JobId = "job-1",
            UserId = userId,
            Type = JobType.CezSync,
            Status = JobStatus.Enqueued
        };
        var mockDbSet = new List<Job> { job }.BuildMockDbSet();
        _jobRepository.Find(Arg.Any<Expression<Func<Job, bool>>>()).Returns(mockDbSet);

        var result = await _sut.GetLatestJobAsync(userId, JobType.CezSync);

        result.Should().NotBeNull();
        result!.JobId.Should().Be("job-1");
    }

    [Test]
    public async Task CreateJobAsync_ShouldSaveAndReturnJob()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.CreateJobAsync(userId, JobType.QuizGeneration);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(JobStatus.Enqueued);
        result.Type.Should().Be(JobType.QuizGeneration);
        result.JobId.Should().Be("pending");

        await _jobRepository.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UpdateJobAsync_ShouldSaveAndNotify_WithoutJobId()
    {
        var userId = Guid.NewGuid();
        var job = new Job { JobId = "job-x", UserId = userId, Status = JobStatus.Enqueued };

        await _sut.UpdateJobAsync(job, JobStatus.Processing);

        job.Status.Should().Be(JobStatus.Processing);
        job.JobId.Should().Be("job-x");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _notificationService.Received(1).SendJobStatusUpdateAsync(userId, "job-x", JobStatus.Processing, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UpdateJobAsync_ShouldSaveAndNotify_WithJobId()
    {
        var userId = Guid.NewGuid();
        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued };

        await _sut.UpdateJobAsync(job, JobStatus.Processing, "job-new");

        job.JobId.Should().Be("job-new");
        job.Status.Should().Be(JobStatus.Processing);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _notificationService.Received(1).SendJobStatusUpdateAsync(userId, "job-new", JobStatus.Processing, Arg.Any<CancellationToken>());
    }
}
