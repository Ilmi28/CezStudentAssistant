using CezStudentAssistant.Application.Commands.Chat;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
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

using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

namespace CezStudentAssistant.UnitTests.Application.Commands.Chat;

[TestFixture]
public class UpdateChatThreadResourcesCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IChatThreadRepository _chatThreadRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private UpdateChatThreadResourcesCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _chatThreadRepository = Substitute.For<IChatThreadRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();

        _unitOfWork.Repository<IChatThreadRepository>().Returns(_chatThreadRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);

        _sut = new UpdateChatThreadResourcesCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public void Handle_ShouldThrowNotFoundException_WhenThreadDoesNotExist()
    {
        var threadId = Guid.NewGuid();
        var emptyMock = new List<ChatThread>().BuildMockDbSet();
        _chatThreadRepository.Find(Arg.Any<Expression<Func<ChatThread, bool>>>()).Returns(emptyMock);

        var command = new UpdateChatThreadResourcesCommand
        {
            UserId = Guid.NewGuid(),
            ChatThreadId = threadId,
            ResourceIds = [Guid.NewGuid()]
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public void Handle_ShouldThrowForbiddenException_WhenUserDoesNotOwnThread()
    {
        var threadId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var thread = new ChatThread
        {
            Id = threadId,
            UserId = ownerId,
            CourseId = Guid.NewGuid(),
            Title = "Test Thread"
        };

        var mockThreads = new List<ChatThread> { thread }.BuildMockDbSet();
        _chatThreadRepository.Find(Arg.Any<Expression<Func<ChatThread, bool>>>()).Returns(mockThreads);

        var command = new UpdateChatThreadResourcesCommand
        {
            UserId = userId,
            ChatThreadId = threadId,
            ResourceIds = []
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<ForbiddenException>();
    }

    [Test]
    public async Task Handle_ShouldUpdateAttachedResources_WhenThreadAndResourcesExist()
    {
        var threadId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        var thread = new ChatThread
        {
            Id = threadId,
            UserId = userId,
            CourseId = courseId,
            Title = "Test Thread",
            Course = new CourseEntity { Id = courseId, Name = "Test Course" }
        };

        var resource = new Resource
        {
            Id = resourceId,
            CourseId = courseId,
            Name = "lecture.pdf",
            DisplayName = "Lecture PDF",
            MimeType = "application/pdf"
        };

        var mockThreads = new List<ChatThread> { thread }.BuildMockDbSet();
        var mockResources = new List<Resource> { resource }.BuildMockDbSet();

        _chatThreadRepository.Find(Arg.Any<Expression<Func<ChatThread, bool>>>()).Returns(mockThreads);
        _resourceRepository.Find(Arg.Any<Expression<Func<Resource, bool>>>()).Returns(mockResources);

        var command = new UpdateChatThreadResourcesCommand
        {
            UserId = userId,
            ChatThreadId = threadId,
            ResourceIds = [resourceId]
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AttachedResourceIds.Should().ContainSingle().Which.Should().Be(resourceId);

        await _chatThreadRepository.Received(1).UpdateAsync(thread, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
