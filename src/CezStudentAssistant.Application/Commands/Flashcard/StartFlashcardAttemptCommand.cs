using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Flashcard;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public sealed class StartFlashcardAttemptCommand : ICommand<FlashcardAttemptDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
    public int CardCount { get; set; }
}

public class StartFlashcardAttemptCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<StartFlashcardAttemptCommand, FlashcardAttemptDto>
{
    protected override string SuccessMessage => FlashcardMessageConsts.StartFlashcardAttemptSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.StartFlashcardAttemptError;

    protected override async Task<FlashcardAttemptDto> ExecuteAsync(StartFlashcardAttemptCommand command, CancellationToken ct)
    {
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var deck = await deckRepo.GetByIdAsync(command.DeckId, ct)
            ?? throw new NotFoundException(FlashcardMessageConsts.FlashcardDeckNotFound);

        if (deck.UserId != command.UserId)
        {
            throw new ForbiddenException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        var attemptRepo = unitOfWork.Repository<IFlashcardAttemptRepository>();

        var attempt = new FlashcardAttempt
        {
            UserId = command.UserId,
            DeckId = command.DeckId,
            Status = QuizAttemptStatus.InProgress,
            CardCount = CezStudentAssistant.Application.Helpers.FlashcardProgressCalculationHelper.DetermineAttemptCardCount(
                command.CardCount,
                deck.CardCountPerAttempt,
                deck.Cards.Count),
            StartedAt = DateTime.UtcNow
        };

        await attemptRepo.AddAsync(attempt, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new FlashcardAttemptDto
        {
            Id = attempt.Id,
            UserId = attempt.UserId,
            DeckId = attempt.DeckId,
            Status = attempt.Status,
            CardCount = attempt.CardCount,
            MasteredCount = 0,
            LearningCount = 0,
            ProgressPercentage = 0,
            StartedAt = attempt.StartedAt
        };
    }
}
