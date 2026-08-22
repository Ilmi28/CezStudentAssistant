using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
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

namespace CezStudentAssistant.UnitTests.Application.Commands.Quiz;

[TestFixture]
public class GenerateQuizCommandHandlerTests
{
    private IJobScheduler _jobScheduler = null!;
    private IJobService _jobService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepo = null!;
    private IQuizRepository _quizRepo = null!;
    private GenerateQuizCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _jobScheduler = Substitute.For<IJobScheduler>();
        _jobService = Substitute.For<IJobService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepo = Substitute.For<ICourseRepository>();
        _quizRepo = Substitute.For<IQuizRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepo);
        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepo);

        _quizRepo.Find(Arg.Any<Expression<Func<CezStudentAssistant.Domain.Entities.Quiz, bool>>>())
            .Returns(new List<CezStudentAssistant.Domain.Entities.Quiz>().AsQueryable());

        _sut = new GenerateQuizCommandHandler(_jobScheduler, _jobService, _unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
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
            TimeLimitMinutes = 15,
            AdditionalInstructions = "test"
        };
        
        var course = new CezStudentAssistant.Domain.Entities.Course { Id = courseId, Name = "Test Course" };
        _courseRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued, Type = JobType.QuizGeneration };
        _jobService.CreateJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>()).Returns(job);
        _jobScheduler.Enqueue<IQuizGenerationService>(Arg.Any<Expression<Action<IQuizGenerationService>>>()).Returns("job-abc");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();

        await _quizRepo.Received(1).AddAsync(Arg.Is<CezStudentAssistant.Domain.Entities.Quiz>(q => q.CourseId == courseId && q.Status == QuizStatusEnum.Generating && q.TimeLimitMinutes == 15), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _jobScheduler.Received(1).Enqueue<IQuizGenerationService>(Arg.Any<Expression<Action<IQuizGenerationService>>>());
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
        
        var course = new CezStudentAssistant.Domain.Entities.Course { Id = courseId, Name = "Test Course" };
        _courseRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued, Type = JobType.QuizGeneration };
        _jobService.CreateJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>()).Returns(job);

        _jobScheduler.When(x => x.Enqueue<IQuizGenerationService>(Arg.Any<Expression<Action<IQuizGenerationService>>>()))
            .Do(x => throw new Exception("Hangfire Error"));

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
