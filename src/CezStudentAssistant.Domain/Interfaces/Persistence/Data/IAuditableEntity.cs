namespace CezStudentAssistant.Domain.Interfaces.Persistence.Data;

/// <summary>
/// Defines an entity that supports auditing by associating a user identifier with the entity.
/// </summary>
/// <remarks>Implementing this interface enables tracking of which user is responsible for creating or modifying
/// the entity. This is commonly used in audit logging and change tracking scenarios to maintain accountability within
/// the system.</remarks>
public interface IAuditableEntity : IBaseEntity
{
    public Guid? UserId { get; set; }
}
