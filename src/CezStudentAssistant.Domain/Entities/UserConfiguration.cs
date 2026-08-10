using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class UserConfiguration : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsCezConnected { get; set; } = false;
    public UserTheme Theme { get; set; } = UserTheme.Light;
    public UserLanguage Language { get; set; } = UserLanguage.Polish;
}
