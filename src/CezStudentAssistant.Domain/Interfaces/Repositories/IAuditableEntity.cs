namespace CezStudentAssistant.Domain.Interfaces.Repositories;

public interface IAuditableEntity
{
    public Guid? UserId { get; set; }
}
