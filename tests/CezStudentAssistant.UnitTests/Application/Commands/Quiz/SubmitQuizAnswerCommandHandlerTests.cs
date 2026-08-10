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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Quiz;

[TestFixture]
public class SubmitQuizAnswerCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private IQuestionRepository _questionRepository = null!;
    private IQuestionAnswerRepository _questionAnswerRepository = null!;
    private ISelectedQuizOptionRepository _selectedQuizOptionRepository = null!;
    private SubmitQuizAnswerCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();
        _questionRepository = Substitute.For<IQuestionRepository>();
        _questionAnswerRepository = Substitute.For<IQuestionAnswerRepository>();
        _selectedQuizOptionRepository = Substitute.For<ISelectedQuizOptionRepository>();

        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepository);
        _unitOfWork.Repository<IQuestionRepository>().Returns(_questionRepository);
        _unitOfWork.Repository<IQuestionAnswerRepository>().Returns(_questionAnswerRepository);
        _unitOfWork.Repository<ISelectedQuizOptionRepository>().Returns(_selectedQuizOptionRepository);

        _sut = new SubmitQuizAnswerCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldSaveAnswerAndOptions_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();

        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = quizAttemptId,
            QuestionId = questionId,
            QuestionOptionIds = new List<Guid> { optionId1, optionId2 }
        };

        var quizAttempt = new QuizAttempt
        {
            Id = quizAttemptId,
            UserId = userId,
            QuizId = quizId,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var question = new Question
        {
            Id = questionId,
            QuizId = quizId,
            Content = "Multiple choice question?",
            Type = QuestionType.MultipleChoice,
            Points = 10m
        };

        var option1 = new QuestionOption { Id = optionId1, QuestionId = questionId, Content = "Opt 1", IsCorrect = true };
        var option2 = new QuestionOption { Id = optionId2, QuestionId = questionId, Content = "Opt 2", IsCorrect = false };
        question.Options = new List<QuestionOption> { option1, option2 };

        _quizAttemptRepository.GetByIdAsync(quizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        _questionRepository.GetByIdAsync(questionId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<Question, object>>[]>())
            .Returns(question);

        _questionAnswerRepository.ExistsAsync(Arg.Any<Expression<Func<QuestionAnswer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.AnswerSubmittedSuccess);

        await _questionAnswerRepository.Received(1).AddAsync(Arg.Is<QuestionAnswer>(x => 
            x.QuizAttemptId == quizAttemptId && x.QuestionId == questionId && x.EarnedPoints == null), Arg.Any<CancellationToken>());

        await _selectedQuizOptionRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<SelectedQuizOption>>(opts => 
            opts.Count() == 2 && 
            opts.Any(o => o.QuestionOptionId == optionId1) && 
            opts.Any(o => o.QuestionOptionId == optionId2)), Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenQuizAttemptDoesNotExist()
    {
        // Arrange
        var command = new SubmitQuizAnswerCommand
        {
            UserId = Guid.NewGuid(),
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid()
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns((QuizAttempt?)null);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{QuizMessageConsts.QuizAttemptNotFound}*");
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenException_WhenQuizAttemptDoesNotBelongToUser()
    {
        // Arrange
        var command = new SubmitQuizAnswerCommand
        {
            UserId = Guid.NewGuid(),
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid()
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = Guid.NewGuid(), // different user
            Status = QuizAttemptStatus.InProgress
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage($"*{QuizMessageConsts.UnauthorizedAttemptAccess}*");
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenQuizAttemptNotInProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid()
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = userId,
            Status = QuizAttemptStatus.Completed // Completed, not InProgress
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"*{QuizMessageConsts.QuizAttemptNotInProgress}*");
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenQuizAttemptExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid()
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = userId,
            Status = QuizAttemptStatus.InProgress,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5) // Expired
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"*{QuizMessageConsts.QuizAttemptExpired}*");
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenQuestionDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid()
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = userId,
            Status = QuizAttemptStatus.InProgress
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        _questionRepository.GetByIdAsync(command.QuestionId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<Question, object>>[]>())
            .Returns((Question?)null);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{QuizMessageConsts.QuestionNotFound}*");
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenQuestionDoesNotBelongToQuiz()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid()
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = userId,
            QuizId = Guid.NewGuid(),
            Status = QuizAttemptStatus.InProgress
        };

        var question = new Question
        {
            Id = command.QuestionId,
            QuizId = Guid.NewGuid(), // different quiz
            Content = "Test"
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        _questionRepository.GetByIdAsync(command.QuestionId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<Question, object>>[]>())
            .Returns(question);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"*{QuizMessageConsts.QuestionNotBelongToQuiz}*");
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenQuestionAlreadyAnswered()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid()
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = userId,
            QuizId = quizId,
            Status = QuizAttemptStatus.InProgress
        };

        var question = new Question
        {
            Id = command.QuestionId,
            QuizId = quizId,
            Content = "Test"
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        _questionRepository.GetByIdAsync(command.QuestionId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<Question, object>>[]>())
            .Returns(question);

        _questionAnswerRepository.ExistsAsync(Arg.Any<Expression<Func<QuestionAnswer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true); // already answered

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"*{QuizMessageConsts.QuestionAlreadyAnswered}*");
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenOptionDoesNotExistForQuestion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var invalidOptionId = Guid.NewGuid();
        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            QuestionOptionIds = new List<Guid> { invalidOptionId }
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = userId,
            QuizId = quizId,
            Status = QuizAttemptStatus.InProgress
        };

        var question = new Question
        {
            Id = command.QuestionId,
            QuizId = quizId,
            Content = "Test",
            Options = new List<QuestionOption>() // empty options
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        _questionRepository.GetByIdAsync(command.QuestionId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<Question, object>>[]>())
            .Returns(question);

        _questionAnswerRepository.ExistsAsync(Arg.Any<Expression<Func<QuestionAnswer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"*{QuizMessageConsts.QuestionOptionNotFound}*");
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenMultipleOptionsForSingleChoiceQuestion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var command = new SubmitQuizAnswerCommand
        {
            UserId = userId,
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            QuestionOptionIds = new List<Guid> { optionId1, optionId2 }
        };

        var quizAttempt = new QuizAttempt
        {
            Id = command.QuizAttemptId,
            UserId = userId,
            QuizId = quizId,
            Status = QuizAttemptStatus.InProgress
        };

        var question = new Question
        {
            Id = command.QuestionId,
            QuizId = quizId,
            Content = "Test",
            Type = QuestionType.SingleChoice,
            Options = new List<QuestionOption>
            {
                new QuestionOption { Id = optionId1, Content = "Opt 1" },
                new QuestionOption { Id = optionId2, Content = "Opt 2" }
            }
        };

        _quizAttemptRepository.GetByIdAsync(command.QuizAttemptId, Arg.Any<CancellationToken>())
            .Returns(quizAttempt);

        _questionRepository.GetByIdAsync(command.QuestionId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<Question, object>>[]>())
            .Returns(question);

        _questionAnswerRepository.ExistsAsync(Arg.Any<Expression<Func<QuestionAnswer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"*{QuizMessageConsts.SingleChoiceMultipleOptions}*");
    }
}
