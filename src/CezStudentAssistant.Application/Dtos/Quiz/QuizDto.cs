using System;

namespace CezStudentAssistant.Application.Dtos.Quiz;

public class QuizDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public Guid CourseId { get; set; }
    public required string CourseName { get; set; }
}
