using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class Job : BaseEntity
{
    public Guid UserId { get; set; }
    public required string JobId { get; set; }
    public JobStatus Status { get; set; }
    public JobType Type { get; set; }

    public User? User { get; set; }
}
