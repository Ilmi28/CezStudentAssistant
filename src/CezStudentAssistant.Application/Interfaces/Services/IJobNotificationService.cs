using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IJobNotificationService
{
    Task SendJobStatusUpdateAsync(Guid userId, string jobId, JobStatus newStatus, CancellationToken ct = default);
}
