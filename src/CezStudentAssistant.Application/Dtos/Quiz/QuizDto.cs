using System;

namespace CezStudentAssistant.Application.Dtos.Quiz;

public class QuizDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public Guid CourseId { get; set; }
    public required string CourseName { get; set; }
    public Domain.Enums.QuizStatusEnum Status { get; set; } = Domain.Enums.QuizStatusEnum.Ready;
    public int? TimeLimitMinutes { get; set; }
}
