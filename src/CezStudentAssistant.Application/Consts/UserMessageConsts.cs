namespace CezStudentAssistant.Application.Consts;

public static class UserMessageConsts
{
    public const string GetUserConfigurationSuccess = "Successfully retrieved user configuration.";
    public const string GetUserConfigurationError = "An error occurred while retrieving user configuration.";
    public const string UpdateUserConfigurationSuccess = "Successfully updated user configuration.";
    public const string UpdateUserConfigurationError = "An error occurred while updating user configuration.";
    public const string GetUserUsageSuccess = "Successfully retrieved user usage.";
    public const string GetUserUsageError = "An error occurred while retrieving user usage.";
    public const string UserNotFound = "User not found.";
    public const string MaximumDailyTokensConfigMissing = "Configuration 'Gemini:MaximumDailyTokens' is missing or invalid.";
}

