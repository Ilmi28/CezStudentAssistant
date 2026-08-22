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
    public List<QuizAttemptDto> Attempts { get; set; } = new();
}

public class QuizAttemptDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public QuizAttemptStatus Status { get; set; }
    public decimal? Points { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<QuestionAnswerDto> Answers { get; set; } = new();
}

public class QuestionAnswerDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public QuestionType Type { get; set; }
    public QuestionDifficulty Difficulty { get; set; }
    public decimal Points { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public bool IsCorrect { get; set; }
}
