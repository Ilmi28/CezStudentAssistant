using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Application.Dtos.Flashcard;

public class FlashcardAttemptDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
    public QuizAttemptStatus Status { get; set; }
    public int CardCount { get; set; }
    public int MasteredCount { get; set; }
    public int LearningCount { get; set; }
    public int ProgressPercentage { get; set; }
    public Dictionary<string, FlashcardStateEnum> CardStates { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
