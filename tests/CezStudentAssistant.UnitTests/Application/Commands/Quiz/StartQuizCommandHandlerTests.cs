using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Quiz;

[TestFixture]
public class StartQuizCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private StartQuizCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();

        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepository);

        _sut = new StartQuizCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldStartQuiz_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();
        var command = new StartQuizCommand
        {
            UserId = userId,
            QuizAttemptId = quizAttemptId
        };

        var attempt = new QuizAttempt
        {
            Id = quizAttemptId,
            UserId = userId,
            Status = QuizAttemptStatus.NotStarted
        };

        _quizAttemptRepository.GetByIdAsync(quizAttemptId, Arg.Any<CancellationToken>())
            .Returns(attempt);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.StartQuizSuccess);

        attempt.Status.Should().Be(QuizAttemptStatus.InProgress);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenQuizAttemptDoesNotExist()
    {
        // Arrange
        var command = new StartQuizCommand
        {
            UserId = Guid.NewGuid(),
            QuizAttemptId = Guid.NewGuid()
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns((QuizAttempt?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(QuizMessageConsts.QuizAttemptNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenException_WhenQuizAttemptDoesNotBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();

        var command = new StartQuizCommand
        {
            UserId = userId,
            QuizAttemptId = quizAttemptId
        };

        var attempt = new QuizAttempt
        {
            Id = quizAttemptId,
            UserId = otherUserId,
            Status = QuizAttemptStatus.NotStarted
        };

        _quizAttemptRepository.GetByIdAsync(quizAttemptId, Arg.Any<CancellationToken>())
            .Returns(attempt);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage(QuizMessageConsts.UnauthorizedStartAccess);
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenQuizAttemptNotReady()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();

        var command = new StartQuizCommand
        {
            UserId = userId,
            QuizAttemptId = quizAttemptId
        };

        var attempt = new QuizAttempt
        {
            Id = quizAttemptId,
            UserId = userId,
            Status = QuizAttemptStatus.InProgress
        };

        _quizAttemptRepository.GetByIdAsync(quizAttemptId, Arg.Any<CancellationToken>())
            .Returns(attempt);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(QuizMessageConsts.QuizAttemptNotReady);
    }
}
