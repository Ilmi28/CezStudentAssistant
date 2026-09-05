namespace CezStudentAssistant.Application.Consts;

public static class CourseMessageConsts
{
    public const string GetCoursesSuccess = "Courses retrieved successfully.";
    public const string GetCoursesError = "An error occurred while retrieving courses.";
    public const string CreateCourseSuccess = "Course created successfully.";
    public const string CreateCourseError = "An error occurred while creating course.";
    public const string UpdateCourseSuccess = "Course updated successfully.";
    public const string UpdateCourseError = "An error occurred while updating course.";
    public const string DeleteCourseSuccess = "Course deleted successfully.";
    public const string DeleteCourseError = "An error occurred while deleting course.";
    public const string CourseNotFound = "Course not found.";
    public const string CourseAccessDenied = "Access denied to the course.";
    public const string CourseInvalidType = "Operation not allowed on this course type.";
    public const string GetCourseDetailsSuccess = "Course details retrieved successfully.";
    public const string GetCourseDetailsError = "An error occurred while retrieving course details.";
    public const string DownloadCourseFileSuccess = "File downloaded successfully.";
    public const string DownloadCourseFileError = "An error occurred while downloading course file.";
    public const string UploadCourseFileSuccess = "File uploaded successfully.";
    public const string UploadCourseFileError = "An error occurred while uploading course file.";
    public const string DeleteCourseFileSuccess = "File deleted successfully.";
    public const string DeleteCourseFileError = "An error occurred while deleting course file.";
    public const string ToggleCourseFileVisibilitySuccess = "File visibility toggled successfully.";
    public const string ToggleCourseFileVisibilityError = "An error occurred while toggling file visibility.";
    public const string UnsupportedFileFormat = "Unsupported file format. Please upload PDF, TXT, DOCX, ODT, PPTX, EPUB, JSON or image files.";
    public const string CourseFilesContainerConfigMissing = "Configuration 'BlobContainerSettings:CourseFilesContainer' is missing or empty.";
}
