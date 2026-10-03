using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Quiz;

[TestFixture]
public class DeleteQuizCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizRepository _quizRepository = null!;
    private ICascadeDeleteService _cascadeDeleteService = null!;
    private DeleteQuizCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _cascadeDeleteService = Substitute.For<ICascadeDeleteService>();
        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);
        _sut = new DeleteQuizCommandHandler(_unitOfWork, _cascadeDeleteService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldDeleteQuiz_WhenQuizExistsAndUserIsOwner()
    {
        var quizId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var quiz = new CezStudentAssistant.Domain.Entities.Quiz
        {
            Id = quizId,
            UserId = userId,
            Name = "Test Quiz",
            CourseId = Guid.NewGuid()
        };

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        var command = new DeleteQuizCommand
        {
            QuizId = quizId,
            UserId = userId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        await _cascadeDeleteService.Received(1).DeleteQuizCascadeAsync(quiz, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_ShouldThrowNotFoundException_WhenQuizDoesNotExist()
    {
        var quizId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns((CezStudentAssistant.Domain.Entities.Quiz?)null);

        var command = new DeleteQuizCommand
        {
            QuizId = quizId,
            UserId = Guid.NewGuid()
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public void Handle_ShouldThrowUnauthorizedException_WhenUserIsNotOwner()
    {
        var quizId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var callerId = Guid.NewGuid();

        var quiz = new CezStudentAssistant.Domain.Entities.Quiz
        {
            Id = quizId,
            UserId = ownerId,
            Name = "Test Quiz",
            CourseId = Guid.NewGuid()
        };

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        var command = new DeleteQuizCommand
        {
            QuizId = quizId,
            UserId = callerId
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<UnauthorizedException>();
    }
}
