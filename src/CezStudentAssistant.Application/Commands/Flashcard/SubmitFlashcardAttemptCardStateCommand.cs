using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public sealed class SubmitFlashcardAttemptCardStateCommand : ICommand<Unit>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid AttemptId { get; set; }
    public Guid CardId { get; set; }
    public FlashcardStateEnum State { get; set; }
}

public class SubmitFlashcardAttemptCardStateCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<SubmitFlashcardAttemptCardStateCommand, Unit>
{
    protected override string SuccessMessage => FlashcardMessageConsts.UpdateFlashcardStateSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.UpdateFlashcardStateError;

    protected override async Task<Unit> ExecuteAsync(SubmitFlashcardAttemptCardStateCommand command, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.Repository<IFlashcardAttemptRepository>();
        var attempt = await attemptRepo.GetByIdAsync(command.AttemptId, ct)
            ?? throw new NotFoundException(FlashcardMessageConsts.FlashcardAttemptNotFound);

        if (attempt.UserId != command.UserId)
        {
            throw new ForbiddenException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        var attemptCardRepo = unitOfWork.Repository<IFlashcardAttemptCardRepository>();
        var existingAttemptCard = await attemptCardRepo
            .Find(ac => ac.FlashcardAttemptId == command.AttemptId && ac.FlashcardId == command.CardId)
            .FirstOrDefaultAsync(ct);

        if (existingAttemptCard != null)
        {
            existingAttemptCard.State = command.State;
            await attemptCardRepo.UpdateAsync(existingAttemptCard, ct);
        }
        else
        {
            var newAttemptCard = new FlashcardAttemptCard
            {
                FlashcardAttemptId = command.AttemptId,
                FlashcardId = command.CardId,
                State = command.State
            };
            await attemptCardRepo.AddAsync(newAttemptCard, ct);
        }

        var cardRepo = unitOfWork.Repository<IFlashcardRepository>();
        var card = await cardRepo.GetByIdAsync(command.CardId, ct);
        if (card != null && card.DeckId == attempt.DeckId)
        {
            if (command.State == FlashcardStateEnum.Mastered || card.State != FlashcardStateEnum.Mastered)
            {
                card.State = command.State;
                await cardRepo.UpdateAsync(card, ct);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
