namespace CezStudentAssistant.Application.Requests.Cez;

public class CezCourseRequest : CezBaseRequest
{
    public required string CourseId { get; set; }
}
