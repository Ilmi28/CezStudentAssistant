using CezStudentAssistant.API.Hubs;
using CezStudentAssistant.API.Interfaces;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace CezStudentAssistant.API.Services;

public class SignalRJobNotificationService(IHubContext<CezSyncNotificationHub, ICezSyncNotificationClient> hubContext) : IJobNotificationService
{
    public async Task SendJobStatusUpdateAsync(Guid userId, string jobId, JobStatus newStatus, CancellationToken ct = default)
    {
        await hubContext.Clients.User(userId.ToString()).CezSyncStatusUpdated(jobId, newStatus.ToString());
    }
}
