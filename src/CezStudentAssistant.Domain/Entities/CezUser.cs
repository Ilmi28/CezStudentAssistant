namespace CezStudentAssistant.Domain.Entities;

public class CezUser : BaseEntity
{
    public string? FullName { get; set; }
    public required string Token { get; set; }
    public required string PrivateToken { get; set; }
    public long ExternalUserId { get; set; }
    public Guid UserId { get; set; }
    public required User User { get; set; }
}
