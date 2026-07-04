using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using MediatR;

namespace CezStudentAssistant.Application.Notifications;

public sealed record CezLoginSucceededNotification(Guid UserId) : INotification;

public sealed class SyncCezCoursesOnLoginHandler(IJobScheduler jobScheduler, IUnitOfWork unitOfWork)
    : INotificationHandler<CezLoginSucceededNotification>
{
    public async Task Handle(CezLoginSucceededNotification notification, CancellationToken cancellationToken)
    {
        var jobId = jobScheduler.Enqueue<ICezService>((cezService) => cezService.SyncUserCourses(notification.UserId, cancellationToken));

        var syncJob = new CezSyncJob
        {
            UserId = notification.UserId,
            JobId = jobId,
            Status = JobStatus.Enqueued
        };

        await unitOfWork.Repository<ICezSyncJobRepository>().AddAsync(syncJob, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
