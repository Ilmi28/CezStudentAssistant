using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;

namespace CezStudentAssistant.Application.Services;

public class CezAuthService(ICezApiClient cezApiClient, IUnitOfWork unitOfWork) : ICezAuthService
{
    public async Task<CezLoginResponse> LoginWithCezAsync(
        string userName,
        string password,
        object messageSource,
        CancellationToken ct = default
    )
    {
        var loginResponse = await cezApiClient.LoginToCez(new CezLoginRequest
        {
            UserName = userName,
            Password = password,
        });

        if (!loginResponse.Success)
        {
            throw new BadRequestException(
                new ApiMessage(messageSource, loginResponse.Message ?? CezMessagesConsts.LoginError)
            );
        }

        var userInfoResponse = await cezApiClient.GetSiteInfo(new CezBaseRequest
        {
            Token = loginResponse.Data?.Token ?? string.Empty
        });

        if (!userInfoResponse.Success || userInfoResponse.Data is null)
        {
            throw new BadRequestException(
                new ApiMessage(messageSource, userInfoResponse.Message ?? CezMessagesConsts.GetSiteInfoError)
            );
        }

        var userInfoData = userInfoResponse.Data;
        var cezUserRepo = unitOfWork.Repository<ICezUserRepository>();
        var userRepo = unitOfWork.Repository<IUserRepository>();

        var existingUser = await userRepo.GetSingleAsync(u => u.UserName == userInfoData.UserName, ct);
        Guid finalUserId;
        if (existingUser == null)
        {
            var user = new User
            {
                UserName = userInfoData.UserName ?? userName
            };

            await userRepo.AddAsync(user, ct);
            await cezUserRepo.AddAsync(
                new CezUser
                {
                    FullName = userInfoData.FullName,
                    Token = loginResponse.Data?.Token ?? string.Empty,
                    PrivateToken = loginResponse.Data?.PrivateToken ?? string.Empty,
                    ExternalUserId = userInfoData.ExternalUserId,
                    User = user
                },
                ct
            );
            
            finalUserId = user.Id;
        }
        else
        {
            var existingCezUser = await cezUserRepo.GetSingleAsync(cu => cu.UserId == existingUser.Id, ct)
                ?? throw new NotFoundException(new ApiMessage(messageSource, CezMessagesConsts.CezUserNotFound));

            existingCezUser.Token = loginResponse.Data?.Token ?? string.Empty;
            existingCezUser.PrivateToken = loginResponse.Data?.PrivateToken ?? string.Empty;
            await cezUserRepo.UpdateAsync(existingCezUser, ct);
            
            finalUserId = existingUser.Id;
        }

        await unitOfWork.SaveChangesAsync(ct);
        
        loginResponse.UserId = finalUserId;
        return loginResponse;
    }
}
