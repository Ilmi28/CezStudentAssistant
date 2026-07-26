using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.AI.Quiz;
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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Services;

[TestFixture]
public class AIServiceTests
{
    private IAIClient _aiClient = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IFileService _fileService = null!;
    private IJobService _jobService = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private IQuizRepository _quizRepository = null!;
    private AIService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _aiClient = Substitute.For<IAIClient>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _fileService = Substitute.For<IFileService>();
        _jobService = Substitute.For<IJobService>();
        
        _courseRepository = Substitute.For<ICourseRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);
        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);

        _sut = new AIService(_aiClient, _unitOfWork, _fileService, _jobService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task GenerateQuiz_ShouldTransitionStatusToSucceededAndSaveQuiz_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingJob = new Job
        {
            JobId = "job-123",
            UserId = userId,
            Status = JobStatus.Enqueued,
            Type = JobType.QuizGeneration
        };
        _jobService.GetLatestJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>()).Returns(existingJob);

        var course = new Course { Id = courseId, Name = "Test Course", Type = Domain.Enums.CourseType.Cez };
        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = "resource-file.pdf",
            DisplayName = "Resource File",
            MimeType = "application/pdf",
            CourseId = courseId
        };
        var mockResourceDbSet = new List<Resource> { resource }.BuildMockDbSet();
        _resourceRepository.Find(Arg.Any<Expression<Func<Resource, bool>>>()).Returns(mockResourceDbSet);

        var stream = new MemoryStream();
        _fileService.DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(stream);

        var aiQuiz = new AIQuiz
        {
            Title = "Generated Quiz",
            Description = "Quiz Description",
            Questions = new List<AIQuestion>
            {
                new AIQuestion
                {
                    Content = "Question 1?",
                    QuestionType = QuestionType.SingleChoice,
                    Points = 1,
                    Options = new List<AIQuestionOption>
                    {
                        new AIQuestionOption { Content = "Option 1", IsCorrect = true },
                        new AIQuestionOption { Content = "Option 2", IsCorrect = false }
                    }
                }
            }
        };
        _aiClient.GenerateQuizAsync(Arg.Any<AIQuizRequest>())
            .Returns(new AIQuizResponse { Success = true, Data = aiQuiz });

        var dto = new GenerateQuizDto
        {
            UserId = userId,
            CourseId = courseId,
            QuestionCount = 1,
            Language = QuizLanguage.PL,
            AdditionalInstructions = "Additional help"
        };

        await _sut.GenerateQuiz(dto);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _quizRepository.Received(1).AddAsync(Arg.Is<Quiz>(q => q.Name == "Generated Quiz" && q.CourseId == courseId), Arg.Any<CancellationToken>());
        
        await _jobService.Received(1).UpdateJobAsync(existingJob, JobStatus.Processing, null, Arg.Any<CancellationToken>());
        await _jobService.Received(1).UpdateJobAsync(existingJob, JobStatus.Succeeded, null, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GenerateQuiz_ShouldThrowAppException_WhenJobNotFound()
    {
        var userId = Guid.NewGuid();
        _jobService.GetLatestJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>()).Returns((Job?)null);

        var dto = new GenerateQuizDto
        {
            UserId = userId,
            CourseId = Guid.NewGuid(),
            QuestionCount = 1,
            Language = QuizLanguage.PL,
            AdditionalInstructions = null
        };

        Func<Task> act = () => _sut.GenerateQuiz(dto);

        await act.Should().ThrowAsync<AppException>().WithMessage($"*{AIMessageConsts.JobNotFound}*");
    }

    [Test]
    public async Task GenerateQuiz_ShouldTransitionStatusToFailedAndThrow_WhenCourseNotFound()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingJob = new Job
        {
            JobId = "job-123",
            UserId = userId,
            Status = JobStatus.Enqueued,
            Type = JobType.QuizGeneration
        };
        _jobService.GetLatestJobAsync(userId, JobType.QuizGeneration, Arg.Any<CancellationToken>()).Returns(existingJob);
        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns((Course?)null);

        var dto = new GenerateQuizDto
        {
            UserId = userId,
            CourseId = courseId,
            QuestionCount = 1,
            Language = QuizLanguage.PL,
            AdditionalInstructions = null
        };

        Func<Task> act = () => _sut.GenerateQuiz(dto);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{AIMessageConsts.CourseNotFound}*");
        await _jobService.Received(1).UpdateJobAsync(existingJob, JobStatus.Failed, null, Arg.Any<CancellationToken>());
    }
}
