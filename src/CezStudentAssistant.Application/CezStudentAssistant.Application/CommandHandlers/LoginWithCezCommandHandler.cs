using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.CommandHandlers;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CezStudentAssistant.Application.CommandHandlers;

public class LoginWithCezCommandHandler : ValidatableCommandHandler<LoginWithCezCommand, CezLoginResponse>
{
    private readonly ICezApiClient _cezApiClient;
    private readonly IUnitOfWork _unitOfWork;
    public LoginWithCezCommandHandler(
        IValidator<LoginWithCezCommand> validator,
        ICezApiClient cezApiClient,
        IUnitOfWork unitOfWork) : base(validator)
    {
        _cezApiClient = cezApiClient;
        _unitOfWork = unitOfWork;
    }

    protected override ApiMessage SuccessMessage => new ApiMessage(this, CezMessagesConsts.LoginSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, CezMessagesConsts.LoginError);

    protected override ApiMessage ValidationMessage => new ApiMessage(this, CezMessagesConsts.LoginValidationError);
    protected override async Task<CezLoginResponse> ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        var loginResponse = await _cezApiClient.LoginToCez(new Requests.Cez.CezLoginRequest
        {
            UserName = command.UserName,
            Password = command.Password,
        });

        if (!loginResponse.Success)
            throw new BadRequestException(new ApiMessage(this, loginResponse.Message ?? CezMessagesConsts.LoginError));

        var userInfoResponse = await _cezApiClient.GetSiteInfo(new CezBaseRequest
        {
            Token = loginResponse.Data?.Token ?? string.Empty
        });

        if (!userInfoResponse.Success || userInfoResponse.Data is null)
            throw new BadRequestException(new ApiMessage(this, userInfoResponse.Message ?? CezMessagesConsts.GetSiteInfoError));

        var userInfoData = userInfoResponse.Data;

        var cezUserRepo = _unitOfWork.Repository<ICezUserRepository>();
        var userRepo = _unitOfWork.Repository<IUserRepository>();

        var existingUser = await userRepo.GetSingleAsync(u => u.UserName == userInfoData.UserName);
        if (existingUser == null)
        {
            var user = new User
            {
                UserName = userInfoData.UserName ?? command.UserName
            };
            await userRepo.AddAsync(user);
            await cezUserRepo.AddAsync(new CezUser
            {
                FullName = userInfoData.FullName,
                Token = loginResponse.Data?.Token ?? string.Empty,
                PrivateToken = loginResponse.Data?.PrivateToken ?? string.Empty,
                ExternalUserId = userInfoData.ExternalUserId,
                User = user
            });
        }
        else
        {
            var existingCezUser = await cezUserRepo.GetSingleAsync(cu => cu.UserId == existingUser.Id)
                ?? throw new NotFoundException(new ApiMessage(this, CezMessagesConsts.CezUserNotFound));
            existingCezUser.Token = loginResponse.Data?.Token ?? string.Empty;
            existingCezUser.PrivateToken = loginResponse.Data?.PrivateToken ?? string.Empty;
            await cezUserRepo.UpdateAsync(existingCezUser);
        }
        await _unitOfWork.SaveChangesAsync();

        return loginResponse;
    }
}
