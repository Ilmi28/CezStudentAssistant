using CezStudentAssistant.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CezStudentAssistant.Application.Dtos.Flashcard;

public class FlashcardDeckDetailsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public Guid CourseId { get; set; }
    public required string CourseName { get; set; }
    public FlashcardDeckStatusEnum Status { get; set; } = FlashcardDeckStatusEnum.Ready;
    public int? CardCountPerAttempt { get; set; }
    public int? EasyCardCountPerAttempt { get; set; }
    public int? MediumCardCountPerAttempt { get; set; }
    public int? HardCardCountPerAttempt { get; set; }
    public int CardCount { get; set; }
    public int MasteredCardCount { get; set; }
    public int LearningCardCount { get; set; }
    public int NewCardCount { get; set; }
    public int ProgressPercentage { get; set; }
    public List<FlashcardDto> Cards { get; set; } = new();
    public List<FlashcardAttemptDto> Attempts { get; set; } = new();
}
