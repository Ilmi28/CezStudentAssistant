using CezStudentAssistant.Application.Commands.Flashcard;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

namespace CezStudentAssistant.UnitTests.Application.Commands.Flashcard;

[TestFixture]
public class GenerateFlashcardsCommandHandlerTests
{
    private IJobScheduler _jobScheduler = null!;
    private IJobService _jobService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepo = null!;
    private IFlashcardDeckRepository _deckRepo = null!;
    private ICezResourceRepository _resourceRepo = null!;
    private ITokenUsageRepository _tokenUsageRepo = null!;
    private GenerateFlashcardsCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _jobScheduler = Substitute.For<IJobScheduler>();
        _jobService = Substitute.For<IJobService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepo = Substitute.For<ICourseRepository>();
        _deckRepo = Substitute.For<IFlashcardDeckRepository>();
        _resourceRepo = Substitute.For<ICezResourceRepository>();
        _tokenUsageRepo = Substitute.For<ITokenUsageRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepo);
        _unitOfWork.Repository<IFlashcardDeckRepository>().Returns(_deckRepo);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepo);
        _unitOfWork.Repository<ITokenUsageRepository>().Returns(_tokenUsageRepo);

        var emptyDecks = new List<FlashcardDeck>().BuildMockDbSet();
        _deckRepo.Find(Arg.Any<Expression<Func<FlashcardDeck, bool>>>())
            .Returns(emptyDecks);

        var emptyResources = new List<Resource>().BuildMockDbSet();
        _resourceRepo.Find(Arg.Any<Expression<Func<Resource, bool>>>())
            .Returns(emptyResources);

        _sut = new GenerateFlashcardsCommandHandler(_jobScheduler, _jobService, _unitOfWork);
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
        var command = new GenerateFlashcardsCommand
        {
            UserId = userId,
            CourseId = courseId,
            CardCount = 10
        };

        var course = new CourseEntity { Id = courseId, Name = "Test Course" };
        _courseRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var job = new Job { JobId = "pending", UserId = userId, Status = JobStatus.Enqueued, Type = JobType.FlashcardGeneration };
        _jobService.CreateJobAsync(userId, JobType.FlashcardGeneration, Arg.Any<CancellationToken>()).Returns(job);

        _jobScheduler.Enqueue<IFlashcardGenerationService>(Arg.Any<Expression<Action<IFlashcardGenerationService>>>()).Returns("job-123");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _jobService.Received(1).CreateJobAsync(userId, JobType.FlashcardGeneration, Arg.Any<CancellationToken>());
        _jobScheduler.Received(1).Enqueue<IFlashcardGenerationService>(Arg.Any<Expression<Action<IFlashcardGenerationService>>>());
        await _jobService.Received(1).UpdateJobAsync(job, JobStatus.Enqueued, "job-123", Arg.Any<CancellationToken>());
    }
}
