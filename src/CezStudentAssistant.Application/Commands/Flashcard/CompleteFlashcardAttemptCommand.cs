using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Flashcard;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public sealed class CompleteFlashcardAttemptCommand : ICommand<FlashcardAttemptDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid AttemptId { get; set; }
    public int MasteredCount { get; set; }
    public int LearningCount { get; set; }
}

public class CompleteFlashcardAttemptCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<CompleteFlashcardAttemptCommand, FlashcardAttemptDto>
{
    protected override string SuccessMessage => FlashcardMessageConsts.CompleteFlashcardAttemptSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.CompleteFlashcardAttemptError;

    protected override async Task<FlashcardAttemptDto> ExecuteAsync(CompleteFlashcardAttemptCommand command, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.Repository<IFlashcardAttemptRepository>();
        var attempt = await attemptRepo.GetByIdAsync(command.AttemptId, ct)
            ?? throw new NotFoundException(FlashcardMessageConsts.FlashcardAttemptNotFound);

        if (attempt.UserId != command.UserId)
        {
            throw new ForbiddenException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        attempt.MasteredCount = command.MasteredCount;
        attempt.LearningCount = command.LearningCount;
        attempt.Status = QuizAttemptStatus.Completed;
        attempt.CompletedAt = DateTime.UtcNow;

        var total = attempt.CardCount > 0 ? attempt.CardCount : 1;
        attempt.ProgressPercentage = (int)Math.Min(100, Math.Round(((command.MasteredCount * 1.0 + command.LearningCount * 0.5) / total) * 100));

        await attemptRepo.UpdateAsync(attempt, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new FlashcardAttemptDto
        {
            Id = attempt.Id,
            UserId = attempt.UserId,
            DeckId = attempt.DeckId,
            Status = attempt.Status,
            CardCount = attempt.CardCount,
            MasteredCount = attempt.MasteredCount,
            LearningCount = attempt.LearningCount,
            ProgressPercentage = attempt.ProgressPercentage,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt
        };
    }
}
