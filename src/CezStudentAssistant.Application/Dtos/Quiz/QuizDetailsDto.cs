using CezStudentAssistant.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CezStudentAssistant.Application.Dtos.Quiz;

public class QuizDetailsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public Guid CourseId { get; set; }
    public required string CourseName { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public QuestionType Type { get; set; }
    public decimal Points { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public bool IsCorrect { get; set; }
}
