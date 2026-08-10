using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Application.Dtos.User;

public class UserConfigurationDto
{
    public bool IsCezConnected { get; set; }
    public UserTheme Theme { get; set; }
    public UserLanguage Language { get; set; }
}
