using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public class ResetFlashcardDeckProgressCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
}

public class ResetFlashcardDeckProgressCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<ResetFlashcardDeckProgressCommand>
{
    protected override string SuccessMessage => FlashcardMessageConsts.ResetFlashcardDeckProgressSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.ResetFlashcardDeckProgressError;

    protected override async Task ExecuteAsync(ResetFlashcardDeckProgressCommand command, CancellationToken ct)
    {
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var deck = await deckRepo.Find(d => d.Id == command.DeckId)
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(ct);

        if (deck == null)
        {
            throw new NotFoundException(FlashcardMessageConsts.FlashcardDeckNotFound);
        }

        if (deck.UserId != command.UserId)
        {
            throw new UnauthorizedException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        var cardRepo = unitOfWork.Repository<IFlashcardRepository>();
        foreach (var card in deck.Cards)
        {
            card.State = FlashcardStateEnum.New;
            await cardRepo.UpdateAsync(card, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
