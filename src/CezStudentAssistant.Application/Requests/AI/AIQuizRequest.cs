namespace CezStudentAssistant.Application.Requests.AI;

public class AIQuizRequest
{
    public required int QuestionCount { get; set; }
    public IEnumerable<Stream> Files { get; set; } = new List<Stream>();
    public string? AdditionalInstructions { get; set; }
}
