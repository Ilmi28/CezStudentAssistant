using System;
using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Application.Dtos.Quiz;

public class QuizDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public Guid CourseId { get; set; }
    public required string CourseName { get; set; }
    public QuizStatusEnum Status { get; set; } = QuizStatusEnum.Ready;
    public int? TimeLimitMinutes { get; set; }
    public decimal? MaxPoints { get; set; }
    public QuizAttemptStatus? LastAttemptStatus { get; set; }
    public DateTime? LastAttemptExpiresAt { get; set; }
    public decimal? LastAttemptPoints { get; set; }
}
