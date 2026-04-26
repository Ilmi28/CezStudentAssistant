namespace CezStudentAssistant.Domain.Entities;

public class CezUser : BaseEntity
{
    public required string UserName { get; set; }
    public required string Token { get; set; }
    public required string PrivateToken { get; set; }
}
