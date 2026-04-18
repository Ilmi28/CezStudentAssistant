namespace CezStudentAssistant.Domain.Responses;


public readonly struct ApiMessage(object source, string message)
{
    public string Source { get; } = source.GetType().Name
        .Replace("CommandHandler", "")
        .Replace("QueryHandler", "")
        .Replace("Command", "")
        .Replace("Query", "")
        .Replace("Handler", "")
        .ToUpper();
    public string Message { get; } = message;
}
