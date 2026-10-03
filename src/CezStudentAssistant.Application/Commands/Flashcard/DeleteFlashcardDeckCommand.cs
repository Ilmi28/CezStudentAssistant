using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public class DeleteFlashcardDeckCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
}

public class DeleteFlashcardDeckCommandHandler(IUnitOfWork unitOfWork, ICascadeDeleteService cascadeDeleteService) : BaseCommandHandler<DeleteFlashcardDeckCommand>
{
    protected override string SuccessMessage => FlashcardMessageConsts.DeleteFlashcardDeckSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.DeleteFlashcardDeckError;

    protected override async Task ExecuteAsync(DeleteFlashcardDeckCommand command, CancellationToken ct)
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

        await cascadeDeleteService.DeleteFlashcardDeckCascadeAsync(deck, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
