namespace CezStudentAssistant.Domain.Entities;

public class User : BaseEntity
{
    public required string UserName { get; set; }
    public string? PasswordHash { get; set; }

    public CezUser? CezUser { get; set; }
    public UserConfiguration? Configuration { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
