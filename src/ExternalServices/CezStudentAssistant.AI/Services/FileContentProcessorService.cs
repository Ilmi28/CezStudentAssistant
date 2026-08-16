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
using UglyToad.PdfPig;

namespace CezStudentAssistant.AI.Services;

public class FileContentProcessorService : IFileContentProcessorService
{
    private static readonly Regex StyleTagRegex = new(@"<style[^>]*>[\s\S]*?</style>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ScriptTagRegex = new(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HeadTagRegex = new(@"<head[^>]*>[\s\S]*?</head>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SvgTagRegex = new(@"<svg[^>]*>[\s\S]*?</svg>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CommentRegex = new(@"<!--[\s\S]*?-->", RegexOptions.Compiled);
    private static readonly Regex BlockTagRegex = new(@"</?(p|w:p|text:p|a:p|h[1-6]|div|br|w:br|text:line-break|tr|li|blockquote|table|section|article)[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AllTagsRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex MultipleSpacesRegex = new(@"[ \t]+", RegexOptions.Compiled);

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

        if (mime == "application/pdf" || (bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46))
        {
            var pdfText = TryExtractTextFromPdf(bytes);
            if (!string.IsNullOrWhiteSpace(pdfText))
            {
                var formattedText = $"--- Content of attached PDF file ---\n{pdfText}\n--- End of file ---";
                return new ProcessedFileContent(null, formattedText, "application/pdf", IsTextFormat: true);
            }

            return new ProcessedFileContent(bytes, null, "application/pdf", IsTextFormat: false);
        }

        if (IsMediaFormat(mime))
        {
            return new ProcessedFileContent(bytes, null, mime, IsTextFormat: false);
        }

        if (bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B)
        {
            var text = TryExtractTextFromZipArchive(bytes);
            if (!string.IsNullOrWhiteSpace(text))
            {
                var formattedText = $"--- Content of attached document ({mime}) ---\n{text}\n--- End of file ---";
                return new ProcessedFileContent(null, formattedText, mime, IsTextFormat: true);
            }
        }

        if (mime == "application/rtf" || mime == "text/rtf" || (bytes.Length >= 5 && Encoding.ASCII.GetString(bytes, 0, 5) == "{\\rtf"))
        {
            var rtfText = TryExtractTextFromRtf(bytes);
            if (!string.IsNullOrWhiteSpace(rtfText))
            {
                var formattedText = $"--- Content of attached RTF document ---\n{rtfText}\n--- End of file ---";
                return new ProcessedFileContent(null, formattedText, mime, IsTextFormat: true);
            }
        }

        if (IsUtf8Text(bytes) || mime.StartsWith("text/") || mime == "application/json" || mime == "application/xml")
        {
            var rawText = Encoding.UTF8.GetString(bytes);
            var formattedText = $"--- Content of attached file ({mime}) ---\n{rawText}\n--- End of file ---";
            return new ProcessedFileContent(null, formattedText, mime, IsTextFormat: true);
        }

        var fallbackText = $"[Attached file: {mime} - binary format unsupported for direct AI ingestion]";
        return new ProcessedFileContent(null, fallbackText, mime, IsTextFormat: true);
    }

    private static bool IsMediaFormat(string mime) =>
        mime.StartsWith("image/") || mime.StartsWith("audio/") || mime.StartsWith("video/");

    private static bool IsUtf8Text(byte[] bytes)
    {
        var sampleLength = Math.Min(bytes.Length, 1024);
        for (var i = 0; i < sampleLength; i++)
        {
            if (bytes[i] == 0) return false;
        }
        return true;
    }

    private static string? TryExtractTextFromPdf(byte[] bytes)
    {
        try
        {
            using var document = PdfDocument.Open(new MemoryStream(bytes));
            var sb = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                if (!string.IsNullOrWhiteSpace(page.Text))
                {
                    sb.AppendLine(page.Text);
                }
            }

            var text = sb.ToString().Trim();
            return text.Length > 20 ? text : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryExtractTextFromZipArchive(byte[] bytes)
    {
        try
        {
            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);

            var epubText = ExtractEpubContent(archive);
            if (!string.IsNullOrWhiteSpace(epubText)) return epubText;

            var docxText = ExtractDocxContent(archive);
            if (!string.IsNullOrWhiteSpace(docxText)) return docxText;

            var pptxText = ExtractPptxContent(archive);
            if (!string.IsNullOrWhiteSpace(pptxText)) return pptxText;

            var odtEntry = archive.GetEntry("content.xml");
            if (odtEntry != null)
            {
                using var reader = new StreamReader(odtEntry.Open(), Encoding.UTF8);
                var text = CleanXml(reader.ReadToEnd());
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractEpubContent(ZipArchive archive)
    {
        var htmlEntries = archive.Entries
            .Where(e => (e.FullName.EndsWith(".xhtml", StringComparison.OrdinalIgnoreCase)
                      || e.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                      || e.FullName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)
                      || (e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && !e.FullName.EndsWith("container.xml", StringComparison.OrdinalIgnoreCase) && !e.FullName.EndsWith("toc.ncx", StringComparison.OrdinalIgnoreCase)))
                      && !e.FullName.StartsWith("META-INF/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (htmlEntries.Count == 0) return null;

        var sb = new StringBuilder();
        foreach (var entry in htmlEntries)
        {
            try
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                var raw = reader.ReadToEnd();
                var cleaned = CleanXml(raw);
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    sb.AppendLine(cleaned);
                    sb.AppendLine();
                }
            }
            catch
            {
            }
        }

        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static string? ExtractDocxContent(ZipArchive archive)
    {
        var docEntry = archive.GetEntry("word/document.xml");
        if (docEntry == null) return null;

        var sb = new StringBuilder();
        try
        {
            using (var reader = new StreamReader(docEntry.Open(), Encoding.UTF8))
            {
                sb.AppendLine(CleanXml(reader.ReadToEnd()));
            }

            var footnotesEntry = archive.GetEntry("word/footnotes.xml");
            if (footnotesEntry != null)
            {
                using var reader = new StreamReader(footnotesEntry.Open(), Encoding.UTF8);
                sb.AppendLine(CleanXml(reader.ReadToEnd()));
            }
        }
        catch
        {
        }

        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static string? ExtractPptxContent(ZipArchive archive)
    {
        var slideEntries = archive.Entries
            .Where(e => e.FullName.StartsWith("ppt/slides/slide", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => ExtractSlideNumber(e.FullName))
            .ToList();

        if (slideEntries.Count == 0) return null;

        var sb = new StringBuilder();
        var slideIndex = 1;
        foreach (var entry in slideEntries)
        {
            try
            {
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                var text = CleanXml(reader.ReadToEnd());
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine($"--- Slide {slideIndex} ---");
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
                slideIndex++;
            }
            catch
            {
            }
        }

        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static int ExtractSlideNumber(string fullName)
    {
        var match = Regex.Match(fullName, @"slide(\d+)\.xml", RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var num) ? num : int.MaxValue;
    }

    private static string? TryExtractTextFromRtf(byte[] bytes)
    {
        try
        {
            var rtf = Encoding.UTF8.GetString(bytes);
            var text = Regex.Replace(rtf, @"{\*?\\[^{}]+}|[{}]|\\\n?[A-Za-z]+\n?(?:-?\d+)? ?|\\'[0-9a-fA-F]{2}", " ");
            return text.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string CleanXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return string.Empty;

        var noStyle = StyleTagRegex.Replace(xml, " ");
        var noScript = ScriptTagRegex.Replace(noStyle, " ");
        var noHead = HeadTagRegex.Replace(noScript, " ");
        var noSvg = SvgTagRegex.Replace(noHead, " ");
        var noComments = CommentRegex.Replace(noSvg, " ");

        var withNewlines = BlockTagRegex.Replace(noComments, "\n");
        var rawText = AllTagsRegex.Replace(withNewlines, " ");
        var decoded = WebUtility.HtmlDecode(rawText);

        var lines = decoded.Split('\n')
            .Select(l => MultipleSpacesRegex.Replace(l, " ").Trim())
            .Where(l => l.Length > 0);

        return string.Join("\n", lines);
    }
}
