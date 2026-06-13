namespace CezStudentAssistant.Cez.Requests;

internal class ExternalCezCourseRequest : ExternalCezBaseRequest
{
    public required string CourseId { get; set; }
}
