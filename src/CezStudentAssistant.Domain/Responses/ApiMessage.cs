namespace CezStudentAssistant.Domain.Responses;


public readonly struct ApiMessage(string code, string message)
{
    public string Code { get; } = code;

    public string Message { get; } = message;
}
