namespace CezStudentAssistant.Application.Helpers;

public static class SupportedFileFormatsHelper
{
    public static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".odt", ".pptx", ".odp", ".epub", ".rtf", ".html", ".htm",
        ".txt", ".md", ".csv", ".tsv", ".json", ".xml", ".yaml", ".yml",
        ".cs", ".js", ".ts", ".jsx", ".tsx", ".py", ".java", ".c", ".cpp", ".h", ".hpp", ".sql", ".sh", ".ps1", ".css",
        ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp",
        ".mp3", ".wav", ".ogg", ".m4a",
        ".mp4", ".webm", ".avi", ".mov"
    };

    public static readonly HashSet<string> SupportedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/json",
        "application/xml",
        "application/rtf",
        "text/rtf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.oasis.opendocument.text",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/vnd.oasis.opendocument.presentation",
        "application/epub+zip"
    };

    public static bool IsSupported(string? mimeType, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(mimeType))
        {
            var trimmedMime = mimeType.Trim().ToLowerInvariant();
            if (SupportedMimeTypes.Contains(trimmedMime) ||
                trimmedMime.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                trimmedMime.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                trimmedMime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ||
                trimmedMime.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var extension = System.IO.Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(extension) && SupportedExtensions.Contains(extension))
            {
                return true;
            }
        }

        return false;
    }
}
