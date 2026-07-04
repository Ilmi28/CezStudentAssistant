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
    IUnitOfWork unitOfWork,
    IJobNotificationService notificationService) : BaseCommandHandler<SyncCezCoursesCommand>
{
    protected override ApiMessage SuccessMessage => new(this, CezMessagesConsts.SyncCoursesSuccess);

    protected override ApiMessage ErrorMessage => new(this, CezMessagesConsts.GetUserCoursesError);

    protected override async Task ExecuteAsync(SyncCezCoursesCommand command, CancellationToken ct)
    {
        var syncJob = new CezSyncJob
        {
            UserId = command.UserId,
            JobId = "pending", // Will be updated after enqueue
            Status = Domain.Enums.JobStatus.Enqueued
        };

        await unitOfWork.Repository<ICezSyncJobRepository>().AddAsync(syncJob, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var jobId = jobScheduler.Enqueue<ICezService>(service => service.SyncUserCourses(command.UserId, ct));

        syncJob.JobId = jobId;
        await unitOfWork.SaveChangesAsync(ct);
        
        await notificationService.SendJobStatusUpdateAsync(command.UserId, jobId, Domain.Enums.JobStatus.Enqueued, ct);
    }
}
