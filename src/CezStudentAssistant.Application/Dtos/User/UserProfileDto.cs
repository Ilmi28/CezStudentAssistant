namespace CezStudentAssistant.Application.Dtos.User;

public class UserProfileDto
{
    public required string UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public bool IsCezConnected { get; set; }
    public string? CezUsername { get; set; }
    public string? CezFullName { get; set; }
    public string? CezEmail { get; set; }
}
