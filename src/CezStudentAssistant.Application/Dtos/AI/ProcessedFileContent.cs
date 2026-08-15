namespace CezStudentAssistant.Application.Dtos.AI;

public record ProcessedFileContent(
    byte[]? Bytes,
    string? Text,
    string MimeType,
    bool IsTextFormat
);
