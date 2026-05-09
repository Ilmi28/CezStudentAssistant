namespace CezStudentAssistant.Domain.Entities;

public class User : BaseEntity
{
    public required string UserName { get; set; }
    public string? PasswordHash { get; set; }
    public CezUser? CezUser { get; set; }
}
