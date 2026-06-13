namespace CezStudentAssistant.Application.Consts;

internal static class AuthMessagesConsts
{
    public const string RegistrationSuccess = "Successfully registered.";
    public const string RegistrationError = "Failed to register.";
    public const string RegistrationValidationError = "Invalid registration details.";
    public const string RegistrationConflictEmail = "Email already exists.";
    public const string RegistrationConflictUsername = "Username already exists.";

    public const string LoginSuccess = "Successfully logged in.";
    public const string LoginError = "Failed to log in.";
    public const string LoginValidationError = "Invalid login details.";
    public const string LoginInvalidCredentials = "Invalid username or password.";
}
