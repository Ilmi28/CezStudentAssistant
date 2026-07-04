using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Cez;

public sealed class SyncCezCoursesCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
}

public class SyncCezCoursesCommandHandler(
    IJobScheduler jobScheduler,
    IUnitOfWork unitOfWork) : BaseCommandHandler<SyncCezCoursesCommand>
{
    protected override ApiMessage SuccessMessage => new(this, CezMessagesConsts.SyncCoursesSuccess);

    protected override ApiMessage ErrorMessage => new(this, CezMessagesConsts.GetUserCoursesError);

    protected override async Task ExecuteAsync(SyncCezCoursesCommand command, CancellationToken ct)
    {
        var jobId = jobScheduler.Enqueue<ICezService>(service => service.SyncUserCourses(command.UserId, ct));

        var syncJob = new CezSyncJob
        {
            UserId = command.UserId,
            JobId = jobId,
            Status = Domain.Enums.JobStatus.Enqueued
        };

        await unitOfWork.Repository<ICezSyncJobRepository>().AddAsync(syncJob, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
