namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IPasswordService
{
    string CreatePasswordHash(string password);

    bool VerifyPassword(string password, string storedHash);
}
