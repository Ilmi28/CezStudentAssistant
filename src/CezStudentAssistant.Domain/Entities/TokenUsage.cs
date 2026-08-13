using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class TokenUsage : BaseEntity
{
    public Guid UserId { get; set; }
    public UsageTokenType UsageType { get; set; }
    public int UsageCount { get; set; }
}
