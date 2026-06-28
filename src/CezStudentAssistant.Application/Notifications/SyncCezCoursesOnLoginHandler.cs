using CezStudentAssistant.Application.Interfaces.Services;
using MediatR;

namespace CezStudentAssistant.Application.Notifications;

public sealed record CezLoginSucceededNotification(Guid UserId) : INotification;

public sealed class SyncCezCoursesOnLoginHandler(IJobScheduler jobScheduler)
    : INotificationHandler<CezLoginSucceededNotification>
{
    public Task Handle(CezLoginSucceededNotification notification, CancellationToken cancellationToken)
    {
        var jobId = jobScheduler.Enqueue<ICezService>((cezService) => cezService.SyncUserCourses(notification.UserId, cancellationToken));


        return Task.CompletedTask;
    }
}
