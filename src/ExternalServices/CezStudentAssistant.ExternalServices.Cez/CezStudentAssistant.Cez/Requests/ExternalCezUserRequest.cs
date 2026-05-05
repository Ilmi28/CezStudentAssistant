namespace CezStudentAssistant.Cez.Requests;

internal class ExternalCezUserRequest : ExternalCezBaseRequest
{
    public required string UserId { get; set; }
}
