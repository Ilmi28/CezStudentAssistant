using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using CezStudentAssistant.Infrastructure.Persistence.Repositories;
using CezStudentAssistant.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Infrastructure.Services;

[TestFixture]
public class CascadeDeleteServiceTests
{
    private AppDbContext _context = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IFileService _fileService = null!;
    private IConfiguration _configuration = null!;
    private CascadeDeleteService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _unitOfWork.Repository<IQuizRepository>().Returns(new QuizRepository(_context));
        _unitOfWork.Repository<IQuestionRepository>().Returns(new QuestionRepository(_context));
        _unitOfWork.Repository<IQuestionOptionRepository>().Returns(new QuestionOptionRepository(_context));
        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(new QuizAttemptRepository(_context));
        _unitOfWork.Repository<IQuestionAnswerRepository>().Returns(new QuestionAnswerRepository(_context));
        _unitOfWork.Repository<ISelectedQuizOptionRepository>().Returns(new SelectedQuizOptionRepository(_context));
        _unitOfWork.Repository<IFlashcardDeckRepository>().Returns(new FlashcardDeckRepository(_context));
        _unitOfWork.Repository<IFlashcardRepository>().Returns(new FlashcardRepository(_context));
        _unitOfWork.Repository<IFlashcardAttemptRepository>().Returns(new FlashcardAttemptRepository(_context));
        _unitOfWork.Repository<IFlashcardAttemptCardRepository>().Returns(new FlashcardAttemptCardRepository(_context));
        _unitOfWork.Repository<IChatThreadRepository>().Returns(new ChatThreadRepository(_context));
        _unitOfWork.Repository<IChatMessageRepository>().Returns(new ChatMessageRepository(_context));
        _unitOfWork.Repository<ICourseRepository>().Returns(new CourseRepository(_context));
        _unitOfWork.Repository<ICezResourceRepository>().Returns(new CezResourceRepository(_context));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(callInfo => _context.SaveChangesAsync(callInfo.Arg<CancellationToken>()));

        _fileService = Substitute.For<IFileService>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"BlobContainerSettings:CourseFilesContainer", "course-files"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new CascadeDeleteService(_unitOfWork, _fileService, _configuration);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task DeleteQuizCascadeAsync_ShouldSoftDeleteQuizAndAllNestedEntities()
    {
        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Name = "Math Quiz",
            UserId = Guid.NewGuid(),
            CourseId = Guid.NewGuid()
        };
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            Content = "2+2=?"
        };
        var option = new QuestionOption
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Content = "4",
            IsCorrect = true
        };
        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            UserId = quiz.UserId,
            Status = QuizAttemptStatus.Completed
        };
        var answer = new QuestionAnswer
        {
            Id = Guid.NewGuid(),
            QuizAttemptId = attempt.Id,
            QuestionId = question.Id
        };
        var selectedOption = new SelectedQuizOption
        {
            Id = Guid.NewGuid(),
            QuestionAnswerId = answer.Id,
            QuestionOptionId = option.Id
        };

        _context.Quizzes.Add(quiz);
        _context.Questions.Add(question);
        _context.QuestionOptions.Add(option);
        _context.QuizAttempts.Add(attempt);
        _context.QuestionAnswers.Add(answer);
        _context.Set<SelectedQuizOption>().Add(selectedOption);
        await _context.SaveChangesAsync();

        await _sut.DeleteQuizCascadeAsync(quiz, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        var deletedQuiz = await _context.Quizzes.IgnoreQueryFilters().FirstOrDefaultAsync(q => q.Id == quiz.Id);
        var deletedQuestion = await _context.Questions.IgnoreQueryFilters().FirstOrDefaultAsync(q => q.Id == question.Id);
        var deletedOption = await _context.QuestionOptions.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == option.Id);
        var deletedAttempt = await _context.QuizAttempts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == attempt.Id);
        var deletedAnswer = await _context.QuestionAnswers.IgnoreQueryFilters().FirstOrDefaultAsync(ans => ans.Id == answer.Id);
        var deletedSelectedOpt = await _context.Set<SelectedQuizOption>().IgnoreQueryFilters().FirstOrDefaultAsync(so => so.Id == selectedOption.Id);

        deletedQuiz!.DeletedAt.Should().NotBeNull();
        deletedQuestion!.DeletedAt.Should().NotBeNull();
        deletedOption!.DeletedAt.Should().NotBeNull();
        deletedAttempt!.DeletedAt.Should().NotBeNull();
        deletedAnswer!.DeletedAt.Should().NotBeNull();
        deletedSelectedOpt!.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteFlashcardDeckCascadeAsync_ShouldSoftDeleteDeckAndAllCardsAndAttempts()
    {
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            Name = "Spanish Vocabulary",
            UserId = Guid.NewGuid(),
            CourseId = Guid.NewGuid()
        };
        var card = new Flashcard
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            Front = "Hola",
            Back = "Hello"
        };
        var attempt = new FlashcardAttempt
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            UserId = deck.UserId
        };
        var attemptCard = new FlashcardAttemptCard
        {
            Id = Guid.NewGuid(),
            FlashcardAttemptId = attempt.Id,
            FlashcardId = card.Id,
            State = FlashcardStateEnum.Mastered
        };

        _context.FlashcardDecks.Add(deck);
        _context.Flashcards.Add(card);
        _context.FlashcardAttempts.Add(attempt);
        _context.FlashcardAttemptCards.Add(attemptCard);
        await _context.SaveChangesAsync();

        await _sut.DeleteFlashcardDeckCascadeAsync(deck, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        var deletedDeck = await _context.FlashcardDecks.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == deck.Id);
        var deletedCard = await _context.Flashcards.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == card.Id);
        var deletedAttempt = await _context.FlashcardAttempts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == attempt.Id);
        var deletedAttemptCard = await _context.FlashcardAttemptCards.IgnoreQueryFilters().FirstOrDefaultAsync(ac => ac.Id == attemptCard.Id);

        deletedDeck!.DeletedAt.Should().NotBeNull();
        deletedCard!.DeletedAt.Should().NotBeNull();
        deletedAttempt!.DeletedAt.Should().NotBeNull();
        deletedAttemptCard!.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteChatThreadCascadeAsync_ShouldSoftDeleteThreadAndMessages()
    {
        var thread = new ChatThread
        {
            Id = Guid.NewGuid(),
            Title = "Study Group",
            UserId = Guid.NewGuid(),
            CourseId = Guid.NewGuid()
        };
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatThreadId = thread.Id,
            Content = "Hello everyone",
            Role = ChatMessageRole.User
        };

        _context.ChatThreads.Add(thread);
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        await _sut.DeleteChatThreadCascadeAsync(thread, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        var deletedThread = await _context.ChatThreads.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == thread.Id);
        var deletedMessage = await _context.ChatMessages.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == message.Id);

        deletedThread!.DeletedAt.Should().NotBeNull();
        deletedMessage!.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteCourseCascadeAsync_ShouldSoftDeleteCourseAndAllAssociatedEntitiesAndBlobFiles()
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Name = "Physics 101",
            Type = CourseType.User
        };
        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Name = "Quiz 1",
            CourseId = course.Id,
            UserId = Guid.NewGuid()
        };
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            Name = "Deck 1",
            CourseId = course.Id,
            UserId = Guid.NewGuid()
        };
        var thread = new ChatThread
        {
            Id = Guid.NewGuid(),
            Title = "Chat 1",
            CourseId = course.Id,
            UserId = Guid.NewGuid()
        };
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Name = "file1.pdf",
            DisplayName = "Lecture 1",
            MimeType = "application/pdf",
            Source = ResourceSource.User
        };

        _context.Courses.Add(course);
        _context.Quizzes.Add(quiz);
        _context.FlashcardDecks.Add(deck);
        _context.ChatThreads.Add(thread);
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        await _sut.DeleteCourseCascadeAsync(course, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        var deletedCourse = await _context.Courses.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == course.Id);
        var deletedQuiz = await _context.Quizzes.IgnoreQueryFilters().FirstOrDefaultAsync(q => q.Id == quiz.Id);
        var deletedDeck = await _context.FlashcardDecks.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == deck.Id);
        var deletedThread = await _context.ChatThreads.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == thread.Id);
        var deletedResource = await _context.Resources.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == resource.Id);

        deletedCourse!.DeletedAt.Should().NotBeNull();
        deletedQuiz!.DeletedAt.Should().NotBeNull();
        deletedDeck!.DeletedAt.Should().NotBeNull();
        deletedThread!.DeletedAt.Should().NotBeNull();
        deletedResource!.DeletedAt.Should().NotBeNull();

        await _fileService.Received(1).DeleteAsync($"{course.Id}/file1.pdf", "course-files", Arg.Any<CancellationToken>());
    }
}
