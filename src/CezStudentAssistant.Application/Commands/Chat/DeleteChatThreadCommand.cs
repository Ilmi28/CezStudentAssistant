using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Chat;

public sealed class DeleteChatThreadCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid ChatThreadId { get; set; }
}

public class DeleteChatThreadCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<DeleteChatThreadCommand>
{
    protected override string SuccessMessage => ChatConsts.ThreadDeletedSuccess;
    protected override string ErrorMessage => ChatConsts.ThreadDeletedError;

    protected override async Task ExecuteAsync(DeleteChatThreadCommand command, CancellationToken ct)
    {
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();
        var thread = await threadRepo.GetByIdAsync(command.ChatThreadId, ct)
            ?? throw new NotFoundException(ChatConsts.ThreadNotFound);

        if (thread.UserId != command.UserId)
        {
            throw new ForbiddenException(ChatConsts.AccessDenied);
        }

        await threadRepo.DeleteAsync(thread, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
