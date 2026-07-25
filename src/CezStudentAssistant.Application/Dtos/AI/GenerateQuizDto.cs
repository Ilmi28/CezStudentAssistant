using CezStudentAssistant.Application.Enums;
using System;

namespace CezStudentAssistant.Application.Dtos.AI;

public class GenerateQuizDto
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int QuestionCount { get; set; }
    public QuizLanguage Language { get; set; } = QuizLanguage.PL;
    public string? AdditionalInstructions { get; set; }
}
