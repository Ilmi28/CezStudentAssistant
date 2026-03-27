namespace CezStudentAssistant.Domain.Interfaces.Persistence.Data;

public interface IAuditableEntity
{
    public Guid? UserId { get; set; }
}
