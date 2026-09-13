using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Auth;

public sealed record SetPasswordCommand(string NewPassword) : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
}

public class SetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService) : BaseCommandHandler<SetPasswordCommand>
{
    protected override string SuccessMessage => AuthMessagesConsts.SetPasswordSuccess;

    protected override string ErrorMessage => AuthMessagesConsts.SetPasswordError;

    protected override async Task ExecuteAsync(SetPasswordCommand command, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(command.UserId, ct)
            ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new ConflictException(AuthMessagesConsts.AlreadyHasPassword);
        }

        user.PasswordHash = passwordService.CreatePasswordHash(command.NewPassword);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
