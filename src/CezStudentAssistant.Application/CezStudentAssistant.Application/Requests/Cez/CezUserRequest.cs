namespace CezStudentAssistant.Application.Requests.Cez;

public class CezUserRequest : CezBaseRequest
{
    public required string UserId { get; set; }
}
