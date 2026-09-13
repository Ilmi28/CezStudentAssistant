using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Notifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Cez;

public sealed record ConnectCezCommand(string UserName, string Password) : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
}

public class ConnectCezCommandHandler(
    ICezService cezService,
    IPublisher publisher) : BaseCommandHandler<ConnectCezCommand>
{
    protected override string SuccessMessage => CezMessagesConsts.ConnectSuccess;

    protected override string ErrorMessage => CezMessagesConsts.ConnectError;

    protected override async Task ExecuteAsync(ConnectCezCommand command, CancellationToken ct)
    {
        await cezService.ConnectCezAsync(command.UserId, command.UserName, command.Password, ct);
        await publisher.Publish(new CezLoginSucceededNotification(command.UserId), ct);
    }
}
