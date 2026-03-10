using CezStudentAssistant.Domain.Interfaces.Common;
using CezStudentAssistant.Domain.Interfaces.Services;

namespace CezStudentAssistant.Infrastructure.Services;

public class PasswordService : IPasswordService, ISingletonService
{
    public string CreatePasswordHash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string storedHash) => BCrypt.Net.BCrypt.Verify(password, storedHash);
}
