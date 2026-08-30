using System;

namespace CezStudentAssistant.Application.Dtos.User;

public class RecentActivityDto
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public double? ScorePercentage { get; set; }
    public decimal? EarnedPoints { get; set; }
    public decimal? MaxPoints { get; set; }
    public int? MasteredCount { get; set; }
    public int? TotalCount { get; set; }
    public DateTime AttemptDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
