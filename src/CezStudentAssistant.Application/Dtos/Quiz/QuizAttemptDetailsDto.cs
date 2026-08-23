using CezStudentAssistant.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CezStudentAssistant.Application.Dtos.Quiz;

public class QuizAttemptDetailsDto
{
    public Guid AttemptId { get; set; }
    public Guid QuizId { get; set; }
    public required string DisplayName { get; set; }
    public required string CourseName { get; set; }
    public QuizAttemptStatus Status { get; set; }
    public bool IsPending { get; set; }
    public decimal? Points { get; set; }
    public decimal? MaxPoints { get; set; }
    public int QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
    public List<QuestionAnswerDto> Answers { get; set; } = new();
}
