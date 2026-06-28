namespace CezStudentAssistant.Application.Requests.Cez;

public class CezFileRequest : CezBaseRequest
{
    public required string FileUrl { get; set; }
}
