using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
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
public class UpdateQuizCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizRepository _quizRepository = null!;
    private UpdateQuizCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizRepository = Substitute.For<IQuizRepository>();

        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);
        _sut = new UpdateQuizCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldUpdateQuiz_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var quiz = new CezStudentAssistant.Domain.Entities.Quiz
        {
            Id = quizId,
            UserId = userId,
            Name = "Old Name",
            DisplayName = "Old Display Name",
            CourseId = Guid.NewGuid(),
            TimeLimitMinutes = 10
        };

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        var command = new UpdateQuizCommand
        {
            UserId = userId,
            QuizId = quizId,
            DisplayName = "New Quiz Name",
            TimeLimitMinutes = 25
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.UpdateQuizSuccess);

        quiz.DisplayName.Should().Be("New Quiz Name");
        quiz.Name.Should().Be("New Quiz Name");
        quiz.TimeLimitMinutes.Should().Be(25);

        await _quizRepository.Received(1).UpdateAsync(quiz, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenQuizDoesNotExist()
    {
        var quizId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns((CezStudentAssistant.Domain.Entities.Quiz?)null);

        var command = new UpdateQuizCommand
        {
            UserId = Guid.NewGuid(),
            QuizId = quizId,
            DisplayName = "Any Name"
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(QuizMessageConsts.QuizNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIsNotOwner()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var quizId = Guid.NewGuid();

        var quiz = new CezStudentAssistant.Domain.Entities.Quiz
        {
            Id = quizId,
            UserId = ownerId,
            Name = "Quiz Name",
            DisplayName = "Quiz Display Name",
            CourseId = Guid.NewGuid()
        };

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(quiz);

        var command = new UpdateQuizCommand
        {
            UserId = otherUserId,
            QuizId = quizId,
            DisplayName = "Updated Name"
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(QuizMessageConsts.QuizAccessDenied);
    }
}
