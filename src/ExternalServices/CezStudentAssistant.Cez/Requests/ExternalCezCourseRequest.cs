namespace CezStudentAssistant.Cez.Requests;

internal class ExternalCezCourseRequest : ExternalCezBaseRequest
{
    public required long CourseId { get; set; }
}
