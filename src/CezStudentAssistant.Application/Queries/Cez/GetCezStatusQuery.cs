using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Cez;
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

namespace CezStudentAssistant.Application.Queries.Cez;

public sealed class GetCezStatusQuery : IQuery<CezStatusDto>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetCezStatusQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetCezStatusQuery, CezStatusDto>
{
    protected override string SuccessMessage => CezMessagesConsts.GetCezStatusSuccess;
    protected override string ErrorMessage => CezMessagesConsts.GetCezStatusError;

    protected override async Task<CezStatusDto> ExecuteAsync(GetCezStatusQuery query, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(
            query.UserId,
            ct,
            true,
            x => x.CezUser!
        ) ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        var isConnected = user.CezUser != null;
        DateTime? lastSyncAt = null;
        JobStatus? lastSyncStatus = null;

        if (isConnected)
        {
            var jobRepo = unitOfWork.Repository<IJobRepository>();
            var latestJob = await jobRepo.Find(j => j.UserId == query.UserId && j.Type == JobType.CezSync, true)
                .OrderByDescending(j => j.LastModifiedAt != default ? j.LastModifiedAt : j.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (latestJob != null)
            {
                lastSyncAt = latestJob.LastModifiedAt != default ? latestJob.LastModifiedAt : latestJob.CreatedAt;
                lastSyncStatus = latestJob.Status;
            }
        }

        return new CezStatusDto
        {
            IsConnected = isConnected,
            LastSyncAt = lastSyncAt,
            LastSyncStatus = lastSyncStatus
        };
    }
}
