using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Flashcard;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public sealed class CompleteFlashcardAttemptCommand : ICommand<FlashcardAttemptDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid AttemptId { get; set; }
}

public class CompleteFlashcardAttemptCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<CompleteFlashcardAttemptCommand, FlashcardAttemptDto>
{
    protected override string SuccessMessage => FlashcardMessageConsts.CompleteFlashcardAttemptSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.CompleteFlashcardAttemptError;

    protected override async Task<FlashcardAttemptDto> ExecuteAsync(CompleteFlashcardAttemptCommand command, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.Repository<IFlashcardAttemptRepository>();
        var attempt = await attemptRepo.Find(a => a.Id == command.AttemptId)
            .Include(a => a.Cards)
                .ThenInclude(c => c.Flashcard)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(FlashcardMessageConsts.FlashcardAttemptNotFound);

        if (attempt.UserId != command.UserId)
        {
            throw new ForbiddenException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        attempt.Status = QuizAttemptStatus.Completed;
        attempt.CompletedAt = DateTime.UtcNow;

        await attemptRepo.UpdateAsync(attempt, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var cardStatesMap = attempt.Cards.ToDictionary(c => c.FlashcardId.ToString(), c => c.State);
        var masteredCount = attempt.Cards.Count(c => c.State == FlashcardStateEnum.Mastered);
        var learningCount = attempt.Cards.Count(c => c.State == FlashcardStateEnum.Learning);
        var progressPercentage = CezStudentAssistant.Application.Helpers.FlashcardProgressCalculationHelper.CalculateAttemptProgressPercentage(
            attempt.Cards.Select(c => (c.Flashcard?.Difficulty ?? Domain.Enums.QuestionDifficulty.Medium, c.State)));

        return new FlashcardAttemptDto
        {
            Id = attempt.Id,
            UserId = attempt.UserId,
            DeckId = attempt.DeckId,
            Status = attempt.Status,
            CardCount = attempt.CardCount,
            MasteredCount = masteredCount,
            LearningCount = learningCount,
            ProgressPercentage = progressPercentage,
            CardStates = cardStatesMap,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt
        };
    }
}
