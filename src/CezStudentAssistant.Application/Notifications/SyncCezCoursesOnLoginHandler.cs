using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using MediatR;

namespace CezStudentAssistant.Application.Notifications;

public sealed record CezLoginSucceededNotification(Guid UserId) : INotification;

public sealed class SyncCezCoursesOnLoginHandler(IJobScheduler jobScheduler, IUnitOfWork unitOfWork, IJobNotificationService notificationService)
    : INotificationHandler<CezLoginSucceededNotification>
{
    public async Task Handle(CezLoginSucceededNotification notification, CancellationToken cancellationToken)
    {
        var syncJob = new Job
        {
            UserId = notification.UserId,
            JobId = string.Empty,
            Status = JobStatus.Enqueued
        };

        await unitOfWork.Repository<IJobRepository>().AddAsync(syncJob, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var jobId = jobScheduler.Enqueue<ICezService>((cezService) => cezService.SyncUserCourses(notification.UserId, cancellationToken));

        syncJob.JobId = jobId;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationService.SendJobStatusUpdateAsync(notification.UserId, jobId, JobStatus.Enqueued, cancellationToken);
    }
}
