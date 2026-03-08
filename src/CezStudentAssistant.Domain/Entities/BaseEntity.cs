using CezStudentAssistant.Domain.Interfaces.Persistence.Data;

namespace CezStudentAssistant.Domain.Entities;

public abstract class BaseEntity : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
