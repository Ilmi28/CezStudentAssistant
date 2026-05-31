using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.CommandHandlers;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;
using FluentValidation;

namespace CezStudentAssistant.Application.CommandHandlers.Auth;

public class LoginUserCommandHandler : ValidatableCommandHandler<LoginUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenService _tokenService;
    public LoginUserCommandHandler(
        IValidator<LoginUserCommand> validator,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ICurrentUserService currentUserService,
        ITokenService tokenService) : base(validator)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _currentUserService = currentUserService;
        _tokenService = tokenService;
    }

    protected override ApiMessage ValidationMessage => new ApiMessage(this, AuthMessagesConsts.LoginValidationError);

    protected override ApiMessage SuccessMessage => new ApiMessage(this, AuthMessagesConsts.LoginSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, AuthMessagesConsts.LoginError);

    protected async override Task ExecuteAsync(LoginUserCommand command, CancellationToken ct)
    {
        var userRepo = _unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetSingleAsync(x => x.UserName == command.UserName, ct);

        if (user == null || user.PasswordHash == null || !_passwordService.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedException(new ApiMessage(this, AuthMessagesConsts.LoginInvalidCredentials));

        var refreshToken = await _tokenService.HandleRefreshToken(user.Id, ct);
        var accessToken = _tokenService.GenerateAccessToken(user.Id);

        _currentUserService.SetSession(accessToken, refreshToken);
    }
}
