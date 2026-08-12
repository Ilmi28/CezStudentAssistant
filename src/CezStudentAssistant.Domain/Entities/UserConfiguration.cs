using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class UserConfiguration : BaseEntity
{
    public Guid UserId { get; set; }
    public UserTheme Theme { get; set; } = UserTheme.Light;
    public UserLanguage Language { get; set; } = UserLanguage.Polish;

    public User User { get; set; } = null!;
}
