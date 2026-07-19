namespace CezStudentAssistant.Application.Requests.AI;

public class AIFile
{
    public required Stream Stream { get; set; }
    public required string MimeType { get; set; }
}
