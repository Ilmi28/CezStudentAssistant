using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Application.Dtos.Flashcard;

public class FlashcardDto
{
    public Guid Id { get; set; }
    public required string Front { get; set; }
    public required string Back { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public FlashcardStateEnum State { get; set; }
    public Guid DeckId { get; set; }
}
