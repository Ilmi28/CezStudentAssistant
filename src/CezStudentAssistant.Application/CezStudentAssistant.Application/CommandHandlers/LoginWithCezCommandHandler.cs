using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.CommandHandlers;
using CezStudentAssistant.Domain.Entities;
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

    protected override ApiMessage SuccessMessage => new ApiMessage(this, "Successfully logged in with CEZ.");

    protected override ApiMessage ErrorMessage => new ApiMessage(this, "Failed to log in with CEZ.");

    protected override ApiMessage ValidationMessage => new ApiMessage(this, "Invalid login credentials for CEZ.");

    protected override async Task<CezLoginResponse> ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        var loginResponse = await _cezApiClient.LoginToCez(new Requests.Cez.CezLoginRequest
        {
            UserName = command.UserName,
            Password = command.Password,
        });

        if (!loginResponse.Success)
            throw new BadRequestException(new ApiMessage(this, loginResponse.Message ?? "Failed to log in with CEZ."));

        var cezUserRepo = _unitOfWork.Repository<ICezUserRepository>();

        var existingUser = await cezUserRepo.GetSingleAsync(u => u.UserName == command.UserName);
        if (existingUser == null)
        {
            await cezUserRepo.AddAsync(new CezUser
            {
                UserName = command.UserName,
                Token = loginResponse.Data?.Token ?? string.Empty,
                PrivateToken = loginResponse.Data?.PrivateToken ?? string.Empty,
            });
        }
        else
        {
            existingUser.Token = loginResponse.Data?.Token ?? string.Empty;
            existingUser.PrivateToken = loginResponse.Data?.PrivateToken ?? string.Empty;
            await cezUserRepo.UpdateAsync(existingUser);
        }
        await _unitOfWork.SaveChangesAsync();

        return loginResponse;
    }
}
