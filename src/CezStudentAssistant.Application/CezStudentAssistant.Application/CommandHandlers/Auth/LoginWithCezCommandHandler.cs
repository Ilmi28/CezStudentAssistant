using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.CommandHandlers;
using FluentValidation;

namespace CezStudentAssistant.Application.CommandHandlers.Auth;

public class LoginWithCezCommandHandler : ValidatableCommandHandler<LoginWithCezCommand>
{
    private readonly ICezService _cezService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    public LoginWithCezCommandHandler(
        IValidator<LoginWithCezCommand> validator,
        ICezService cezService,
        ITokenService tokenService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork) : base(validator)
    {
        _cezService = cezService;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _currentUserService = currentUserService;

    }

    protected override ApiMessage SuccessMessage => new ApiMessage(this, CezMessagesConsts.LoginSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, CezMessagesConsts.LoginError);

    protected override ApiMessage ValidationMessage => new ApiMessage(this, CezMessagesConsts.LoginValidationError);

    protected override async Task ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        var userId = await _cezService.LoginWithCezAsync(command.UserName, command.Password, ct);

        var refreshToken = await _tokenService.HandleRefreshToken(userId, ct);
        var accessToken = _tokenService.GenerateAccessToken(userId);
        await _unitOfWork.SaveChangesAsync(ct);

        _currentUserService.SetSession(accessToken, refreshToken);
    }
}
