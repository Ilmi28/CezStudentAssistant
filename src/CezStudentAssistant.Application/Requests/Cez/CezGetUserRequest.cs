namespace CezStudentAssistant.Application.Requests.Cez;

public class CezGetUserRequest : CezBaseRequest
{
    public required string Field { get; set; }
    public required string Value { get; set; }
}
