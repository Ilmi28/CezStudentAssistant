using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Services;

public class CezService(ICezApiClient cezApiClient, IUnitOfWork unitOfWork) : ICezService
{
    public async Task<CezUserInfo> LoginWithCezAsync(string userName, string password, CancellationToken ct = default)
    {
        var loginResponse = await cezApiClient.LoginToCez(new CezLoginRequest
        {
            UserName = userName,
            Password = password,
        });

        if (!loginResponse.Success || loginResponse.Data is null)
            throw new BadRequestException(new ApiMessage(this, loginResponse.Message ?? CezMessagesConsts.LoginError));

        var userInfoResponse = await cezApiClient.GetSiteInfo(new CezBaseRequest
        {
            Token = loginResponse.Data.Token
        });

        if (!userInfoResponse.Success || userInfoResponse.Data is null)
            throw new BadRequestException(new ApiMessage(this, userInfoResponse.Message ?? CezMessagesConsts.GetSiteInfoError));

        return new CezUserInfo
        {
            SiteInfo = userInfoResponse.Data,
            Tokens = loginResponse.Data
        };
    }

    public async Task<CezGetSiteInfoResponse> GetSiteInfoAsync(string token, CancellationToken ct = default)
    {
        var userInfoResponse = await cezApiClient.GetSiteInfo(new CezBaseRequest
        {
            Token = token
        });

        return userInfoResponse;
    }

    public async Task<Guid> SyncCezUser(CezUserInfo cezUserInfo, CancellationToken ct = default)
    {
        var cezUserRepo = unitOfWork.Repository<ICezUserRepository>();
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var siteInfoData = cezUserInfo.SiteInfo;
        var tokensData = cezUserInfo.Tokens;

        var existingUser = await userRepo.GetSingleAsync(u => u.UserName == siteInfoData.UserName, ct);
        if (existingUser == null)
        {
            var user = new User
            {
                UserName = siteInfoData.UserName
                    ?? throw new BadRequestException(new ApiMessage(this, CezMessagesConsts.GetSiteInfoError))
            };

            await userRepo.AddAsync(user, ct);
            await cezUserRepo.AddAsync(
                new CezUser
                {
                    FullName = siteInfoData.FullName,
                    Token = tokensData.Token ?? string.Empty,
                    PrivateToken = tokensData.PrivateToken ?? string.Empty,
                    ExternalUserId = siteInfoData.ExternalUserId,
                    User = user
                },
                ct
            );

            return user.Id;
        }

        var existingCezUser = await cezUserRepo.GetSingleAsync(cu => cu.UserId == existingUser.Id, ct)
                ?? throw new NotFoundException(new ApiMessage(this, CezMessagesConsts.CezUserNotFound));

        existingCezUser.Token = tokensData.Token ?? string.Empty;
        existingCezUser.PrivateToken = tokensData.PrivateToken ?? string.Empty;
        await cezUserRepo.UpdateAsync(existingCezUser, ct);

        return existingUser.Id;
    }
}
