using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.DTOs;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using CezStudentAssistant.Domain.Interfaces.Persistence.Repositories;
using CezStudentAssistant.Domain.Responses;
using FluentValidation;

namespace CezStudentAssistant.Domain.CommandHandlers;

public class RegisterUserCommandHandler : ValidatableCommandHandler<RegisterUserCommand, Guid>,
    ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public RegisterUserCommandHandler(IValidator<RegisterUserCommand> validator, IUnitOfWork unitOfWork) : base(validator)
    {
        _unitOfWork = unitOfWork;
    }

    protected override ApiMessage ValidationMessage => UserApiMessage.RegisterUserValidation;

    protected override ApiMessage SuccessMessage => UserApiMessage.RegisterUserSuccess;

    protected override ApiMessage ErrorMessage => UserApiMessage.RegisterUserError;

    protected async override Task<Guid> ExecuteAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var userRepo = _unitOfWork.Repository<IUserRepository>();

        var userWithEmailExists = await userRepo.ExistsAsync(x => x.Email == command.Email);
        if (!userWithEmailExists)
            throw new ConflictException(UserApiMessage.RegisterUserEmailExists);

        var userWithUserNameExists = await userRepo.ExistsAsync(x => x.UserName == command.UserName);
        if (!userWithUserNameExists)
            throw new ConflictException(UserApiMessage.RegisterUserUserNameExists);

        return Guid.NewGuid();
    }
}
