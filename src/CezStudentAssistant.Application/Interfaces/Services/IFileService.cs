namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IFileService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string fileName, string containerName, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string fileName, string containerName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string fileName, string containerName, CancellationToken cancellationToken = default);
}
