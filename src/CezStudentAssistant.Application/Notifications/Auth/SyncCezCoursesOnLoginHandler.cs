using CezStudentAssistant.Application.Interfaces.Services;
using MediatR;

namespace CezStudentAssistant.Application.Notifications.Auth;

public sealed record CezLoginSucceededNotification(Guid UserId) : INotification;

public sealed class SyncCezCoursesOnLoginHandler(ICezService cezService)
    : INotificationHandler<CezLoginSucceededNotification>
{
    public Task Handle(CezLoginSucceededNotification notification, CancellationToken cancellationToken)
        => cezService.SyncUserCourses(notification.UserId, cancellationToken);
}
