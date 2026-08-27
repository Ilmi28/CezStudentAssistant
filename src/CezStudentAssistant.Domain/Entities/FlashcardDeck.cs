using CezStudentAssistant.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CezStudentAssistant.Domain.Entities;

public class FlashcardDeck : BaseEntity
{
    public required string Name { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public FlashcardDeckStatusEnum Status { get; set; } = FlashcardDeckStatusEnum.Ready;
    public int? CardCountPerAttempt { get; set; }
    public int? EasyCardCountPerAttempt { get; set; }
    public int? MediumCardCountPerAttempt { get; set; }
    public int? HardCardCountPerAttempt { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<Flashcard> Cards { get; set; } = new List<Flashcard>();
    public ICollection<FlashcardAttempt> Attempts { get; set; } = new List<FlashcardAttempt>();
}
