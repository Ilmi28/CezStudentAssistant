using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Domain.Entities;

public class FlashcardAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;
    public int CardCount { get; set; }
    public int MasteredCount { get; set; }
    public int LearningCount { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public FlashcardDeck Deck { get; set; } = null!;
    public User User { get; set; } = null!;
}
