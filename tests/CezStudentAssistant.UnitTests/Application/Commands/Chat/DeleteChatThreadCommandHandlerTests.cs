using CezStudentAssistant.Application.Commands.Chat;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.Application.Commands.Chat;

[TestFixture]
public class DeleteChatThreadCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IChatThreadRepository _chatThreadRepository = null!;
    private ICascadeDeleteService _cascadeDeleteService = null!;
    private DeleteChatThreadCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _chatThreadRepository = Substitute.For<IChatThreadRepository>();
        _cascadeDeleteService = Substitute.For<ICascadeDeleteService>();

        _unitOfWork.Repository<IChatThreadRepository>().Returns(_chatThreadRepository);
        _sut = new DeleteChatThreadCommandHandler(_unitOfWork, _cascadeDeleteService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldDeleteThread_WhenUserIsOwner()
    {
        var threadId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var thread = new ChatThread { Id = threadId, UserId = userId, Title = "Test Thread" };

        _chatThreadRepository.GetByIdAsync(threadId, Arg.Any<CancellationToken>()).Returns(thread);

        var command = new DeleteChatThreadCommand { UserId = userId, ChatThreadId = threadId };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        await _cascadeDeleteService.Received(1).DeleteChatThreadCascadeAsync(thread, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_ShouldThrowForbiddenException_WhenUserIsNotOwner()
    {
        var threadId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var thread = new ChatThread { Id = threadId, UserId = ownerId, Title = "Test Thread" };

        _chatThreadRepository.GetByIdAsync(threadId, Arg.Any<CancellationToken>()).Returns(thread);

        var command = new DeleteChatThreadCommand { UserId = otherUserId, ChatThreadId = threadId };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<ForbiddenException>();
    }
}
