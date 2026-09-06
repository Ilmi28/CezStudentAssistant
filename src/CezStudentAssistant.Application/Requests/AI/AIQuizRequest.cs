using CezStudentAssistant.Application.Enums;

namespace CezStudentAssistant.Application.Requests.AI;

public class AIQuizRequest
{
    public required int QuestionCount { get; set; }
    public QuizLanguage Language { get; set; } = QuizLanguage.PL;
    public IEnumerable<AIFile> Files { get; set; } = new List<AIFile>();
    public string? AdditionalInstructions { get; set; }
    public int? EasyCount { get; set; }
    public int? MediumCount { get; set; }
    public int? HardCount { get; set; }
    public bool GenerateFromPromptOnly { get; set; }
}
