using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.CommandHandlers;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;
using FluentValidation;

namespace CezStudentAssistant.Application.CommandHandlers.Auth;

public class LoginUserCommandHandler : ValidatableCommandHandler<LoginUserCommand, Guid>,
    ICommandHandler<LoginUserCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    public LoginUserCommandHandler(
        IValidator<LoginUserCommand> validator,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService) : base(validator)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    protected override ApiMessage ValidationMessage => new ApiMessage(this, AuthMessagesConsts.LoginValidationError);

    protected override ApiMessage SuccessMessage => new ApiMessage(this, AuthMessagesConsts.LoginSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, AuthMessagesConsts.LoginError);

    protected async override Task<Guid> ExecuteAsync(LoginUserCommand command, CancellationToken ct)
    {
        var userRepo = _unitOfWork.Repository<IUserRepository>();

        var user = await userRepo.GetSingleAsync(x => x.UserName == command.UserName, ct);
        if (user == null || user.PasswordHash == null || !_passwordService.VerifyPassword(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(new ApiMessage(this, AuthMessagesConsts.LoginInvalidCredentials));
        }

        return user.Id;
    }
}
