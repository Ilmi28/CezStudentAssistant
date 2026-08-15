using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.AI.Services;

public class FileContentProcessorService : IFileContentProcessorService
{
    public async Task<ProcessedFileContent?> ProcessFileAsync(AIFile file, CancellationToken cancellationToken = default)
    {
        if (file.Stream == null) return null;

        if (file.Stream.CanSeek) file.Stream.Position = 0;
        using var ms = new MemoryStream();
        await file.Stream.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();
        if (file.Stream.CanSeek) file.Stream.Position = 0;

        if (bytes.Length == 0) return null;

        var mime = file.MimeType?.Trim().ToLowerInvariant() ?? string.Empty;

        if (IsSupportedByGemini(mime))
        {
            var targetMime = mime.StartsWith("text/") ? "text/plain" : mime;
            return new ProcessedFileContent(bytes, null, targetMime, IsTextFormat: false);
        }

        var text = TryExtractTextFromZip(bytes);
        if (!string.IsNullOrWhiteSpace(text))
        {
            var formattedText = $"--- Content of attached file ({mime}) ---\n{text}\n--- End of file ---";
            return new ProcessedFileContent(null, formattedText, mime, IsTextFormat: true);
        }

        var fallbackText = $"[Attached file: {mime} - binary format unsupported for direct AI ingestion]";
        return new ProcessedFileContent(null, fallbackText, mime, IsTextFormat: true);
    }

    private static bool IsSupportedByGemini(string mime) =>
        mime == "application/pdf" || mime == "application/json" ||
        mime.StartsWith("text/") || mime.StartsWith("image/") ||
        mime.StartsWith("audio/") || mime.StartsWith("video/");

    private static string? TryExtractTextFromZip(byte[] bytes)
    {
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B) return null;

        try
        {
            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            var entry = archive.GetEntry("content.xml") ?? archive.GetEntry("word/document.xml");

            if (entry != null)
            {
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                return CleanXml(reader.ReadToEnd());
            }

            var slideTexts = archive.Entries
                .Where(e => e.FullName.StartsWith("ppt/slides/slide", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".xml"))
                .Select(e => { using var r = new StreamReader(e.Open(), Encoding.UTF8); return CleanXml(r.ReadToEnd()); })
                .Where(t => !string.IsNullOrWhiteSpace(t));

            var result = string.Join("\n", slideTexts);
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
        catch { return null; }
    }

    private static string CleanXml(string xml)
    {
        var withNewlines = Regex.Replace(xml, @"</?(p|w:p|text:p|br|w:br|text:line-break)[^>]*>", "\n", RegexOptions.IgnoreCase);
        var rawText = Regex.Replace(withNewlines, @"<[^>]+>", " ");
        var decoded = WebUtility.HtmlDecode(rawText);
        return string.Join("\n", decoded.Split('\n').Select(l => Regex.Replace(l, @"\s+", " ").Trim()).Where(l => l.Length > 0));
    }
}
