namespace CezStudentAssistant.Cez.Responses;

internal class ExternalCezGetUserCoursesResponse
{
    public required string ExternalId { get; set; }
    public string? ShortName { get; set; }
    public string? FullName { get; set; }
    public string? DisplayName { get; set; }
    public string? CourseImage { get; set; }

}
