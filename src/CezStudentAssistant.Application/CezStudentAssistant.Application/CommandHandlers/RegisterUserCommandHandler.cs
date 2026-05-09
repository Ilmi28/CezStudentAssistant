using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.CommandHandlers;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;
using FluentValidation;

namespace CezStudentAssistant.Application.CommandHandlers;

public class RegisterUserCommandHandler : ValidatableCommandHandler<RegisterUserCommand, Guid>,
    ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    public RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService) : base(validator)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    protected override ApiMessage ValidationMessage => new ApiMessage(this, AuthMessagesConsts.RegistrationValidationError);

    protected override ApiMessage SuccessMessage => new ApiMessage(this, AuthMessagesConsts.RegistrationSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, AuthMessagesConsts.RegistrationError);
    protected async override Task<Guid> ExecuteAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var userRepo = _unitOfWork.Repository<IUserRepository>();

        var userWithUserNameExists = await userRepo.ExistsAsync(x => x.UserName == command.UserName, ct);
        if (userWithUserNameExists)
            throw new ConflictException(new ApiMessage(this, AuthMessagesConsts.RegistrationConflictUsername));
        var user = new User
        {
            UserName = command.UserName,
            PasswordHash = _passwordService.CreatePasswordHash(command.Password)
        };

        await userRepo.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync();

        return user.Id;
    }
}
