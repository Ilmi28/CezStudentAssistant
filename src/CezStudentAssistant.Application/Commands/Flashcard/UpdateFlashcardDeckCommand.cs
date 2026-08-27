using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public class UpdateFlashcardDeckCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
    public required string Name { get; set; }
    public int? CardCountPerAttempt { get; set; }
    public int? EasyCardCountPerAttempt { get; set; }
    public int? MediumCardCountPerAttempt { get; set; }
    public int? HardCardCountPerAttempt { get; set; }
}

public class UpdateFlashcardDeckCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<UpdateFlashcardDeckCommand>
{
    protected override string SuccessMessage => FlashcardMessageConsts.UpdateFlashcardDeckSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.UpdateFlashcardDeckError;

    protected override async Task ExecuteAsync(UpdateFlashcardDeckCommand command, CancellationToken ct)
    {
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var deck = await deckRepo.GetByIdAsync(command.DeckId, ct);

        if (deck == null)
        {
            throw new NotFoundException(FlashcardMessageConsts.FlashcardDeckNotFound);
        }

        if (deck.UserId != command.UserId)
        {
            throw new UnauthorizedException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        deck.Name = command.Name;
        deck.CardCountPerAttempt = command.CardCountPerAttempt;
        deck.EasyCardCountPerAttempt = command.EasyCardCountPerAttempt;
        deck.MediumCardCountPerAttempt = command.MediumCardCountPerAttempt;
        deck.HardCardCountPerAttempt = command.HardCardCountPerAttempt;

        await deckRepo.UpdateAsync(deck, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
