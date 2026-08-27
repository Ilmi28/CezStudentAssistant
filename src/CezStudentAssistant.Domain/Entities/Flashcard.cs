using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Domain.Entities;

public class Flashcard : BaseEntity
{
    public required string Front { get; set; }
    public required string Back { get; set; }
    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;
    public FlashcardStateEnum State { get; set; } = FlashcardStateEnum.New;
    public Guid DeckId { get; set; }

    public FlashcardDeck Deck { get; set; } = null!;
}
