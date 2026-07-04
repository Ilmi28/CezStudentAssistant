using CezStudentAssistant.Application.Commands.Cez;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Application.Commands.Cez;

public class SyncCezCoursesCommandHandlerTests
{
    private IJobScheduler _jobScheduler = null!;
    private IUnitOfWork _unitOfWork = null!;
    private ICezSyncJobRepository _syncJobRepository = null!;
    private IJobNotificationService _notificationService = null!;
    private SyncCezCoursesCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _jobScheduler = Substitute.For<IJobScheduler>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _syncJobRepository = Substitute.For<ICezSyncJobRepository>();
        _notificationService = Substitute.For<IJobNotificationService>();
        
        _unitOfWork.Repository<ICezSyncJobRepository>().Returns(_syncJobRepository);

        _sut = new SyncCezCoursesCommandHandler(_jobScheduler, _unitOfWork, _notificationService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCallJobSchedulerAndSaveJob_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SyncCezCoursesCommand { UserId = userId };
        _jobScheduler.Enqueue<ICezService>(Arg.Any<Expression<Action<ICezService>>>()).Returns("job-123");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
        
        _jobScheduler.Received(1).Enqueue<ICezService>(Arg.Any<Expression<Action<ICezService>>>());
        await _syncJobRepository.Received(1).AddAsync(Arg.Is<CezSyncJob>(j => j.UserId == userId && j.Status == Domain.Enums.JobStatus.Enqueued && j.JobId == "job-123"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        
        await _notificationService.Received(1).SendJobStatusUpdateAsync(userId, "job-123", Domain.Enums.JobStatus.Enqueued, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowException_WhenJobSchedulerThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SyncCezCoursesCommand { UserId = userId };
        _jobScheduler.When(x => x.Enqueue<ICezService>(Arg.Any<Expression<Action<ICezService>>>()))
            .Do(x => throw new Exception("Hangfire Error"));

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
