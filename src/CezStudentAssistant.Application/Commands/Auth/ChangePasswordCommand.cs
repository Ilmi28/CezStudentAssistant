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

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
}

public class ChangePasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService) : BaseCommandHandler<ChangePasswordCommand>
{
    protected override string SuccessMessage => AuthMessagesConsts.ChangePasswordSuccess;

    protected override string ErrorMessage => AuthMessagesConsts.ChangePasswordError;

    protected override async Task ExecuteAsync(ChangePasswordCommand command, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(command.UserId, ct)
            ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new BadRequestException(AuthMessagesConsts.DoesNotHavePassword);
        }

        if (!passwordService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException(AuthMessagesConsts.InvalidCurrentPassword);
        }

        user.PasswordHash = passwordService.CreatePasswordHash(command.NewPassword);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
