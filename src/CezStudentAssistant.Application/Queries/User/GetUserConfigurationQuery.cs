using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.User;

public sealed class GetUserConfigurationQuery : IQuery<UserConfigurationDto>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetUserConfigurationQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetUserConfigurationQuery, UserConfigurationDto>
{
    protected override string SuccessMessage => UserMessageConsts.GetUserConfigurationSuccess;
    protected override string ErrorMessage => UserMessageConsts.GetUserConfigurationError;

    protected override async Task<UserConfigurationDto> ExecuteAsync(GetUserConfigurationQuery query, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(
            query.UserId,
            ct,
            true,
            x => x.CezUser!,
            x => x.Configuration!
        ) ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        var isCezConnected = user.CezUser != null && !user.CezUser.IsDisabled;
        DateTime? lastCezSync = null;

        if (isCezConnected)
        {
            var jobRepo = unitOfWork.Repository<IJobRepository>();
            var latestJob = await jobRepo.Find(j => j.UserId == query.UserId && j.Type == JobType.CezSync, true)
                .OrderByDescending(j => j.LastModifiedAt != default ? j.LastModifiedAt : j.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (latestJob != null)
            {
                lastCezSync = latestJob.LastModifiedAt != default ? latestJob.LastModifiedAt : latestJob.CreatedAt;
            }
        }

        var theme = user.Configuration?.Theme ?? UserTheme.Light;
        var language = user.Configuration?.Language ?? UserLanguage.Polish;

        return new UserConfigurationDto
        {
            IsCezConnected = isCezConnected,
            HasPassword = user.PasswordHash != null,
            LastCezSync = lastCezSync,
            Theme = theme,
            Language = language
        };
    }
}
