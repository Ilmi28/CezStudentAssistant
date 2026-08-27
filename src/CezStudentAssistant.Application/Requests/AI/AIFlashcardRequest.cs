using CezStudentAssistant.Application.Enums;
using System.Collections.Generic;

namespace CezStudentAssistant.Application.Requests.AI;

public class AIFlashcardRequest
{
    public required int CardCount { get; set; }
    public QuizLanguage Language { get; set; } = QuizLanguage.PL;
    public IEnumerable<AIFile> Files { get; set; } = new List<AIFile>();
    public string? AdditionalInstructions { get; set; }
    public int? EasyCount { get; set; }
    public int? MediumCount { get; set; }
    public int? HardCount { get; set; }
}
