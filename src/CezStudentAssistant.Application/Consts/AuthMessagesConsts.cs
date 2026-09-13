namespace CezStudentAssistant.Application.Consts;

public static class AuthMessagesConsts
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

    public const string RefreshSuccess = "Token refreshed successfully.";
    public const string RefreshError = "Failed to refresh token.";
    public const string InvalidRefreshToken = "Invalid or expired refresh token.";

    public const string SetPasswordSuccess = "Password set successfully.";
    public const string SetPasswordError = "Failed to set password.";
    public const string AlreadyHasPassword = "User account already has a password set. Use Change Password instead.";
    public const string ChangePasswordSuccess = "Password changed successfully.";
    public const string ChangePasswordError = "Failed to change password.";
    public const string InvalidCurrentPassword = "Current password is incorrect.";
    public const string DoesNotHavePassword = "User account does not have a password set. Use Set Password instead.";
}
