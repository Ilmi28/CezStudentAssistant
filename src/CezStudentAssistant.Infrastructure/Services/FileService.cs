using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.Services;

namespace CezStudentAssistant.Infrastructure.Services;

public class FileService(BlobServiceClient blobServiceClient) : IFileService, IScopedService
{
    public async Task<bool> DeleteAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var response = await blobClient.DownloadAsync(cancellationToken);
        var memoryStream = new MemoryStream();
        await response.Value.Content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<bool> ExistsAsync(string fileName, string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }
}
