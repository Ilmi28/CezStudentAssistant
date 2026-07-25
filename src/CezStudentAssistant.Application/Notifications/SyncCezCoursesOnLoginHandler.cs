using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Notifications;

public sealed record CezLoginSucceededNotification(Guid UserId) : INotification;

public sealed class SyncCezCoursesOnLoginHandler(IJobScheduler jobScheduler, IJobService jobService)
    : INotificationHandler<CezLoginSucceededNotification>
{
    public async Task Handle(CezLoginSucceededNotification notification, CancellationToken cancellationToken)
    {
        var job = await jobService.CreateJobAsync(notification.UserId, JobType.CezSync, cancellationToken);

        var jobId = jobScheduler.Enqueue<ICezService>((cezService) => cezService.SyncUserCourses(notification.UserId, cancellationToken));

        await jobService.UpdateJobAsync(job, JobStatus.Enqueued, jobId, cancellationToken);
    }
}
