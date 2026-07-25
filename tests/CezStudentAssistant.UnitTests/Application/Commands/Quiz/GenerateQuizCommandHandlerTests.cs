using CezStudentAssistant.Application.Commands.Quiz;
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

namespace CezStudentAssistant.UnitTests.Application.Commands.Quiz;

[TestFixture]
public class GenerateQuizCommandHandlerTests
{
    private IJobScheduler _jobScheduler = null!;
    private IJobService _jobService = null!;
    private GenerateQuizCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _jobScheduler = Substitute.For<IJobScheduler>();
        _jobService = Substitute.For<IJobService>();

        _sut = new GenerateQuizCommandHandler(_jobScheduler, _jobService);
    }

    [Test]
    public async Task Handle_ShouldCallJobSchedulerAndSaveJob_WhenCommandIsValid()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new GenerateQuizCommand
        {
            UserId = userId,
            CourseId = courseId,
            QuestionCount = 5,
            AdditionalInstructions = "test"
        };
        
        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued, Type = JobType.QuizGeneration };
        _jobService.CreateJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>()).Returns(job);
        _jobScheduler.Enqueue<IAIService>(Arg.Any<Expression<Action<IAIService>>>()).Returns("job-abc");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();

        _jobScheduler.Received(1).Enqueue<IAIService>(Arg.Any<Expression<Action<IAIService>>>());
        await _jobService.Received(1).CreateJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>());
        await _jobService.Received(1).UpdateJobAsync(job, JobStatus.Enqueued, "job-abc", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowException_WhenJobSchedulerThrows()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new GenerateQuizCommand
        {
            UserId = userId,
            CourseId = courseId,
            QuestionCount = 5,
            AdditionalInstructions = "test"
        };
        
        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued, Type = JobType.QuizGeneration };
        _jobService.CreateJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>()).Returns(job);

        _jobScheduler.When(x => x.Enqueue<IAIService>(Arg.Any<Expression<Action<IAIService>>>()))
            .Do(x => throw new Exception("Hangfire Error"));

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
