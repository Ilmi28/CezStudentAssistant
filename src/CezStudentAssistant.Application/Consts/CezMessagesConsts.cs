namespace CezStudentAssistant.Application.Consts;

public static class CezMessagesConsts
{
    public const string LoginSuccess = "Successfully logged in to CEZ.";
    public const string LoginError = "Failed to log in with CEZ.";
    public const string LoginValidationError = "Invalid login credentials for CEZ.";
    public const string GetSiteInfoError = "Failed to retrieve user information from CEZ.";
    public const string CezUserNotFound = "No user found with the given CEZ credentials.";
    public const string CezCourseNotFound = "No course found with the given CEZ course ID.";
    public const string GetUserCoursesError = "Failed to retrieve user courses from CEZ.";
    public const string SyncCoursesSuccess = "Successfully synchronized user courses.";
    public const string GetCourseContentsError = "Failed to retrieve course contents from CEZ.";
    public const string SyncCoursesError = "Failed to synchronize user courses.";
    public const string GetCezStatusSuccess = "Successfully retrieved CEZ status.";
    public const string GetCezStatusError = "Failed to retrieve CEZ status.";
    public const string ConnectSuccess = "Successfully connected account to CEZ.";
    public const string ConnectError = "Failed to connect account to CEZ.";
    public const string AlreadyConnectedToCez = "This account is already connected to CEZ.";
    public const string DisconnectSuccess = "Successfully disconnected account from CEZ.";
    public const string DisconnectError = "Failed to disconnect account from CEZ.";
    public const string NotConnectedToCez = "This account is not connected to CEZ.";
    public const string CannotDisconnectPureCezAccount = "Cannot disconnect a CEZ-only account. Please set a password for your account first.";
    public const string CezAccountAlreadyLinkedToAnotherUser = "This CEZ account is already connected to another user.";
}
