using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Infrastructure.Services;

public class CascadeDeleteService(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IConfiguration configuration) : ICascadeDeleteService
{
    private readonly string? _containerName = configuration["BlobContainerSettings:CourseFilesContainer"];

    public async Task DeleteQuizCascadeAsync(Quiz quiz, CancellationToken ct = default)
    {
        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var questionAnswerRepo = unitOfWork.Repository<IQuestionAnswerRepository>();
        var selectedOptionRepo = unitOfWork.Repository<ISelectedQuizOptionRepository>();
        var questionRepo = unitOfWork.Repository<IQuestionRepository>();
        var questionOptionRepo = unitOfWork.Repository<IQuestionOptionRepository>();
        var quizRepo = unitOfWork.Repository<IQuizRepository>();

        var attempts = await quizAttemptRepo.Find(a => a.QuizId == quiz.Id, false, a => a.Answers).ToListAsync(ct);
        foreach (var attempt in attempts)
        {
            foreach (var answer in attempt.Answers)
            {
                var selectedOptions = await selectedOptionRepo.Find(so => so.QuestionAnswerId == answer.Id).ToListAsync(ct);
                foreach (var opt in selectedOptions)
                {
                    await selectedOptionRepo.DeleteAsync(opt, ct);
                }

                await questionAnswerRepo.DeleteAsync(answer, ct);
            }

            await quizAttemptRepo.DeleteAsync(attempt, ct);
        }

        var questions = await questionRepo.Find(q => q.QuizId == quiz.Id, false, q => q.Options).ToListAsync(ct);
        foreach (var question in questions)
        {
            foreach (var opt in question.Options)
            {
                await questionOptionRepo.DeleteAsync(opt, ct);
            }

            await questionRepo.DeleteAsync(question, ct);
        }

        await quizRepo.DeleteAsync(quiz, ct);
    }

    public async Task DeleteFlashcardDeckCascadeAsync(FlashcardDeck deck, CancellationToken ct = default)
    {
        var attemptRepo = unitOfWork.Repository<IFlashcardAttemptRepository>();
        var attemptCardRepo = unitOfWork.Repository<IFlashcardAttemptCardRepository>();
        var flashcardRepo = unitOfWork.Repository<IFlashcardRepository>();
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();

        var attempts = await attemptRepo.Find(a => a.DeckId == deck.Id, false, a => a.Cards).ToListAsync(ct);
        foreach (var attempt in attempts)
        {
            foreach (var cardAttempt in attempt.Cards)
            {
                await attemptCardRepo.DeleteAsync(cardAttempt, ct);
            }

            await attemptRepo.DeleteAsync(attempt, ct);
        }

        var flashcards = await flashcardRepo.Find(f => f.DeckId == deck.Id).ToListAsync(ct);
        foreach (var card in flashcards)
        {
            await flashcardRepo.DeleteAsync(card, ct);
        }

        await deckRepo.DeleteAsync(deck, ct);
    }

    public async Task DeleteChatThreadCascadeAsync(ChatThread thread, CancellationToken ct = default)
    {
        var messageRepo = unitOfWork.Repository<IChatMessageRepository>();
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();

        var messages = await messageRepo.Find(m => m.ChatThreadId == thread.Id).ToListAsync(ct);
        foreach (var msg in messages)
        {
            await messageRepo.DeleteAsync(msg, ct);
        }

        await threadRepo.DeleteAsync(thread, ct);
    }

    public async Task DeleteCourseCascadeAsync(Course course, CancellationToken ct = default)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();
        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var courseRepo = unitOfWork.Repository<ICourseRepository>();

        var quizzes = await quizRepo.Find(q => q.CourseId == course.Id).ToListAsync(ct);
        foreach (var quiz in quizzes)
        {
            await DeleteQuizCascadeAsync(quiz, ct);
        }

        var decks = await deckRepo.Find(d => d.CourseId == course.Id).ToListAsync(ct);
        foreach (var deck in decks)
        {
            await DeleteFlashcardDeckCascadeAsync(deck, ct);
        }

        var threads = await threadRepo.Find(t => t.CourseId == course.Id).ToListAsync(ct);
        foreach (var thread in threads)
        {
            await DeleteChatThreadCascadeAsync(thread, ct);
        }

        var resources = await resourceRepo.Find(r => r.CourseId == course.Id).ToListAsync(ct);
        foreach (var resource in resources)
        {
            if (!string.IsNullOrEmpty(_containerName) && resource.Source == ResourceSource.User)
            {
                var filePath = $"{course.Id}/{resource.Name}";
                await fileService.DeleteAsync(filePath, _containerName, ct);
            }

            await resourceRepo.DeleteAsync(resource, ct);
        }

        await courseRepo.DeleteAsync(course, ct);
    }
}
