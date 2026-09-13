using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.UserConfiguration;

public sealed class UpdateUserConfigurationCommand : ICommand<UserConfigurationDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public UserTheme? Theme { get; set; }
    public UserLanguage? Language { get; set; }
}

public class UpdateUserConfigurationCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<UpdateUserConfigurationCommand, UserConfigurationDto>
{
    protected override string SuccessMessage => UserMessageConsts.UpdateUserConfigurationSuccess;
    protected override string ErrorMessage => UserMessageConsts.UpdateUserConfigurationError;

    protected override async Task<UserConfigurationDto> ExecuteAsync(UpdateUserConfigurationCommand command, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(
            command.UserId,
            ct,
            false,
            x => x.CezUser!,
            x => x.Configuration!
        ) ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        if (user.Configuration == null)
        {
            user.Configuration = new Domain.Entities.UserConfiguration
            {
                UserId = command.UserId,
                Theme = command.Theme ?? UserTheme.Light,
                Language = command.Language ?? UserLanguage.Polish
            };
            var configRepo = unitOfWork.Repository<IGenericRepository<Domain.Entities.UserConfiguration>>();
            await configRepo.AddAsync(user.Configuration, ct);
        }
        else
        {
            if (command.Theme.HasValue)
            {
                user.Configuration.Theme = command.Theme.Value;
            }
            if (command.Language.HasValue)
            {
                user.Configuration.Language = command.Language.Value;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new UserConfigurationDto
        {
            IsCezConnected = user.CezUser != null,
            HasPassword = user.PasswordHash != null,
            Theme = user.Configuration.Theme,
            Language = user.Configuration.Language
        };
    }
}
