namespace CezStudentAssistant.Application.Consts;

internal static class CezMessagesConsts
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
}
