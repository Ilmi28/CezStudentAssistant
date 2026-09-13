using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Cez;

public sealed class DisconnectCezCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
}

public class DisconnectCezCommandHandler(
    ICezService cezService) : BaseCommandHandler<DisconnectCezCommand>
{
    protected override string SuccessMessage => CezMessagesConsts.DisconnectSuccess;

    protected override string ErrorMessage => CezMessagesConsts.DisconnectError;

    protected override async Task ExecuteAsync(DisconnectCezCommand command, CancellationToken ct)
    {
        await cezService.DisconnectCezAsync(command.UserId, ct);
    }
}
