namespace CezStudentAssistant.Application.Responses.Cez;

public class CezGetUserCoursesResponse
{
    public required string ExternalId { get; set; }
    public string? ShortName { get; set; }
    public string? FullName { get; set; }
    public string? DisplayName { get; set; }
    public string? CourseImage { get; set; }
}
