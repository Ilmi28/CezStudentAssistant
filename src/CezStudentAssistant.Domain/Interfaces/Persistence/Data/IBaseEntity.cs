namespace CezStudentAssistant.Domain.Interfaces.Persistence.Data;

/// <summary>
/// Defines the basic properties for an entity, including unique identification and audit timestamps.
/// </summary>
/// <remarks>Implement this interface to provide consistent identification and auditing information across
/// entities. The audit properties track when the entity was created, last modified, and marked as deleted.</remarks>
public interface IBaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastModifiedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
