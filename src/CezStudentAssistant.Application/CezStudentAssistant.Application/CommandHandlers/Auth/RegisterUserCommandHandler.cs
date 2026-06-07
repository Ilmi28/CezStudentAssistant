using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;

namespace CezStudentAssistant.Application.CommandHandlers.Auth;

public class RegisterUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService) : BaseCommandHandler<RegisterUserCommand>
{
    protected override ApiMessage SuccessMessage => new ApiMessage(this, AuthMessagesConsts.RegistrationSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, AuthMessagesConsts.RegistrationError);

    protected async override Task ExecuteAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();

        var userWithUserNameExists = await userRepo.ExistsAsync(x => x.UserName == command.UserName, ct);
        if (userWithUserNameExists)
            throw new ConflictException(new ApiMessage(this, AuthMessagesConsts.RegistrationConflictUsername));
            
        var user = new User
        {
            UserName = command.UserName,
            PasswordHash = passwordService.CreatePasswordHash(command.Password)
        };

        await userRepo.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync();
    }
}
