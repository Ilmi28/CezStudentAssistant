using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Auth;

public sealed record RegisterUserCommand(string UserName, string Password) : ICommand { }

public class RegisterUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService) : BaseCommandHandler<RegisterUserCommand>
{
    protected override ApiMessage SuccessMessage => AuthMessagesConsts.RegistrationSuccess;

    protected override ApiMessage ErrorMessage => AuthMessagesConsts.RegistrationError;

    protected async override Task ExecuteAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();

        var userWithUserNameExists = await userRepo.ExistsAsync(x => x.UserName == command.UserName, ct);
        if (userWithUserNameExists)
            throw new ConflictException(AuthMessagesConsts.RegistrationConflictUsername);

        var user = new User
        {
            UserName = command.UserName,
            PasswordHash = passwordService.CreatePasswordHash(command.Password)
        };

        await userRepo.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync();
    }
}
