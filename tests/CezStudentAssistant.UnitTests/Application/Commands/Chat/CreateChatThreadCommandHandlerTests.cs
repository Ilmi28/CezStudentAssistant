using CezStudentAssistant.Application.Commands.Chat;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System.Linq.Expressions;

using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

namespace CezStudentAssistant.UnitTests.Application.Commands.Chat;

[TestFixture]
public class CreateChatThreadCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private IChatThreadRepository _chatThreadRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private CreateChatThreadCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _chatThreadRepository = Substitute.For<IChatThreadRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();

        var emptyResources = new List<Resource>().BuildMockDbSet();
        _resourceRepository.Find(Arg.Any<Expression<Func<Resource, bool>>>()).Returns(emptyResources);

        var emptyThreads = new List<ChatThread>().BuildMockDbSet();
        _chatThreadRepository.Find(Arg.Any<Expression<Func<ChatThread, bool>>>()).Returns(emptyThreads);

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<IChatThreadRepository>().Returns(_chatThreadRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);
        _sut = new CreateChatThreadCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCreateThread_WhenCourseExists()
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var course = new CourseEntity { Id = courseId, Name = "Data Structures" };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var command = new CreateChatThreadCommand
        {
            UserId = userId,
            CourseId = courseId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Nowy czat");
        result.Data.CourseName.Should().Be("Data Structures");

        await _chatThreadRepository.Received(1).AddAsync(Arg.Is<ChatThread>(t =>
            t.CourseId == courseId &&
            t.UserId == userId &&
            t.Title == "Nowy czat"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldReuseExistingEmptyThread_WhenEmptyThreadAlreadyExists()
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var course = new CourseEntity { Id = courseId, Name = "Data Structures" };
        var existingEmptyThread = new ChatThread
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            Title = "Nowy czat",
            CreatedAt = DateTime.UtcNow,
            Messages = new List<ChatMessage>()
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);
        var threads = new List<ChatThread> { existingEmptyThread }.BuildMockDbSet();
        _chatThreadRepository.Find(Arg.Any<Expression<Func<ChatThread, bool>>>()).Returns(threads);

        var command = new CreateChatThreadCommand
        {
            UserId = userId,
            CourseId = courseId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingEmptyThread.Id);
        result.Data.CourseName.Should().Be("Data Structures");

        await _chatThreadRepository.DidNotReceive().AddAsync(Arg.Any<ChatThread>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var courseId = Guid.NewGuid();
        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns((CourseEntity?)null);

        var command = new CreateChatThreadCommand
        {
            UserId = Guid.NewGuid(),
            CourseId = courseId
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<NotFoundException>();
    }
}
