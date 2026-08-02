using System;
using System.Linq;

namespace CezStudentAssistant.Application.Responses;


public readonly struct ApiMessage(object? source, string message, string? code = null)
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
    public string Code { get; } = code ?? GenerateCode(message);

    public ApiMessage(string message, string? code = null) : this(null, message, code) { }

    public static implicit operator ApiMessage(string message) => new(message);

    private static string GenerateCode(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "ERROR";

        var cleanChars = message
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '_' || c == '-')
            .ToArray();

        var cleanString = new string(cleanChars);
        var parts = cleanString.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.ToUpperInvariant());

        return string.Join("_", parts);
    }
}
