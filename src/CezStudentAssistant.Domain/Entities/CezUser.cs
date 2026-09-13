namespace CezStudentAssistant.Domain.Entities;

public class CezUser : BaseEntity
{
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public required string Token { get; set; }
    public required string PrivateToken { get; set; }
    public long ExternalUserId { get; set; }
    public Guid UserId { get; set; }
    public bool IsDisabled { get; set; }
    public User User { get; set; } = null!;
}
