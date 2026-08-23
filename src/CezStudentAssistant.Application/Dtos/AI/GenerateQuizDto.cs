using CezStudentAssistant.Application.Enums;
using System;

namespace CezStudentAssistant.Application.Dtos.AI;

public class GenerateQuizDto
{
    public Guid QuizId { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid ReservationId { get; set; }
    public int QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public QuizLanguage Language { get; set; } = QuizLanguage.PL;
    public string? AdditionalInstructions { get; set; }
    public int? EasyQuestionCountPerAttempt { get; set; }
    public int? MediumQuestionCountPerAttempt { get; set; }
    public int? HardQuestionCountPerAttempt { get; set; }
}
