using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Application.Dtos.AI;

public class GenerateFlashcardsDto
{
    public Guid DeckId { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid ReservationId { get; set; }
    public int CardCount { get; set; }
    public QuizLanguage Language { get; set; } = QuizLanguage.PL;
    public string? AdditionalInstructions { get; set; }
    public int? EasyCount { get; set; }
    public int? MediumCount { get; set; }
    public int? HardCount { get; set; }
    public bool GenerateFromPromptOnly { get; set; }
}
