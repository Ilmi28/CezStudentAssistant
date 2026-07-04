namespace CezStudentAssistant.API.Interfaces;

public interface ICezSyncNotificationClient
{
    Task CezSyncStatusUpdated(string jobId, string status);
}
