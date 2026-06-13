namespace CezStudentAssistant.Application.Responses;


public readonly struct ApiMessage(object? source, string message)
{
    public string Source { get; } = (source?.GetType().Name ?? "SYSTEM")
        .Replace("CommandHandler", "")
        .Replace("QueryHandler", "")
        .Replace("Command", "")
        .Replace("Query", "")
        .Replace("Handler", "")
        .Replace("Service", "")
        .ToUpper();
    public string Message { get; } = message;
}
