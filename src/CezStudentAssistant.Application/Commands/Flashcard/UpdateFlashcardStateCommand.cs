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

public class UpdateFlashcardStateCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CardId { get; set; }
    public FlashcardStateEnum State { get; set; }
}

public class UpdateFlashcardStateCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<UpdateFlashcardStateCommand>
{
    protected override string SuccessMessage => FlashcardMessageConsts.UpdateFlashcardStateSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.UpdateFlashcardStateError;

    protected override async Task ExecuteAsync(UpdateFlashcardStateCommand command, CancellationToken ct)
    {
        var cardRepo = unitOfWork.Repository<IFlashcardRepository>();
        var card = await cardRepo.Find(c => c.Id == command.CardId)
            .Include(c => c.Deck)
            .FirstOrDefaultAsync(ct);

        if (card == null)
        {
            throw new NotFoundException(FlashcardMessageConsts.FlashcardNotFound);
        }

        if (card.Deck.UserId != command.UserId)
        {
            throw new UnauthorizedException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        card.State = command.State;
        await cardRepo.UpdateAsync(card, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
