namespace CezStudentAssistant.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    public required DateTime ExpiryTime { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}

