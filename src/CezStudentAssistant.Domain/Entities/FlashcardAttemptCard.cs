using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Domain.Entities;

public class FlashcardAttemptCard : BaseEntity
{
    public Guid FlashcardAttemptId { get; set; }
    public Guid FlashcardId { get; set; }
    public FlashcardStateEnum State { get; set; }

    public FlashcardAttempt FlashcardAttempt { get; set; } = null!;
    public Flashcard Flashcard { get; set; } = null!;
}
