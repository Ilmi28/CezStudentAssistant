namespace CezStudentAssistant.Cez.Requests;

internal class ExternalCezDownloadFileRequest
{
    public required string Token { get; set; }
    public required string FileUrl { get; set; }
}
