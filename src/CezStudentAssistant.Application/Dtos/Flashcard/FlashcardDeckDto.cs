using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Application.Dtos.Flashcard;

public class FlashcardDeckDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public Guid CourseId { get; set; }
    public required string CourseName { get; set; }
    public FlashcardDeckStatusEnum Status { get; set; } = FlashcardDeckStatusEnum.Ready;
    public int CardCount { get; set; }
    public int MasteredCardCount { get; set; }
    public int LearningCardCount { get; set; }
    public int NewCardCount { get; set; }
    public int ProgressPercentage { get; set; }
}
