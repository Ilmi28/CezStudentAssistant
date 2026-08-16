using CezStudentAssistant.AI.Services;
using CezStudentAssistant.Application.Requests.AI;
using FluentAssertions;
using NUnit.Framework;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.AI;

[TestFixture]
public class FileContentProcessorTests
{
    private FileContentProcessorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new FileContentProcessorService();
    }

    [Test]
    public async Task ProcessFileAsync_ShouldReturnBytes_WhenMimeTypeIsSupported()
    {
        var content = "Sample PDF content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var aiFile = new AIFile
        {
            Stream = stream,
            MimeType = "application/pdf"
        };

        var processed = await _service.ProcessFileAsync(aiFile);

        processed.Should().NotBeNull();
        processed!.IsTextFormat.Should().BeFalse();
        processed.Bytes.Should().NotBeNull();
        processed.MimeType.Should().Be("application/pdf");
    }

    [Test]
    public async Task ProcessFileAsync_ShouldExtractText_WhenMimeTypeIsOdt()
    {
        var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><office:document-content><office:body><office:text><text:p>Pytanie quizowe z ODT</text:p></office:text></office:body></office:document-content>";

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("content.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(xmlContent);
        }

        ms.Position = 0;
        var aiFile = new AIFile
        {
            Stream = ms,
            MimeType = "application/vnd.oasis.opendocument.text"
        };

        var processed = await _service.ProcessFileAsync(aiFile);

        processed.Should().NotBeNull();
        processed!.IsTextFormat.Should().BeTrue();
        processed.Text.Should().Contain("Pytanie quizowe z ODT");
        processed.Text.Should().Contain("application/vnd.oasis.opendocument.text");
    }

    [Test]
    public async Task ProcessFileAsync_ShouldExtractText_WhenMimeTypeIsDocx()
    {
        var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><w:document><w:body><w:p><w:r><w:t>Pytanie quizowe z DOCX</w:t></w:r></w:p></w:body></w:document>";

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("word/document.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(xmlContent);
        }

        ms.Position = 0;
        var aiFile = new AIFile
        {
            Stream = ms,
            MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        var processed = await _service.ProcessFileAsync(aiFile);

        processed.Should().NotBeNull();
        processed!.IsTextFormat.Should().BeTrue();
        processed.Text.Should().Contain("Pytanie quizowe z DOCX");
    }

    [Test]
    public async Task ProcessFileAsync_ShouldReturnFallbackText_WhenBinaryFormatIsUnsupported()
    {
        byte[] dummyBinary = [0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE];
        using var stream = new MemoryStream(dummyBinary);
        var aiFile = new AIFile
        {
            Stream = stream,
            MimeType = "application/x-unknown-binary"
        };

        var processed = await _service.ProcessFileAsync(aiFile);

        processed.Should().NotBeNull();
        processed!.IsTextFormat.Should().BeTrue();
        processed.Text.Should().Contain("application/x-unknown-binary");
        processed.Text.Should().Contain("binary format unsupported");
    }

    [Test]
    public async Task ProcessFileAsync_ShouldExtractText_WhenMimeTypeIsEpubWithStylesAndScripts()
    {
        var chapter1 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><style>body { color: red; }</style><script>alert('x');</script></head><body><h1>Wiedźmin</h1><p>Geralt z Rivii przybył do Wyzimy.</p></body></html>";
        var chapter2 = "<html><body><h2>Ostatnie Życzenie</h2><p>Jaskier śpiewał pieśni o miłości.</p></body></html>";

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry1 = archive.CreateEntry("OEBPS/Text/ch01.xhtml");
            using (var writer = new StreamWriter(entry1.Open())) writer.Write(chapter1);

            var entry2 = archive.CreateEntry("OEBPS/Text/ch02.xhtml");
            using (var writer = new StreamWriter(entry2.Open())) writer.Write(chapter2);
        }

        ms.Position = 0;
        var aiFile = new AIFile
        {
            Stream = ms,
            MimeType = "application/epub+zip"
        };

        var processed = await _service.ProcessFileAsync(aiFile);

        processed.Should().NotBeNull();
        processed!.IsTextFormat.Should().BeTrue();
        processed.Text.Should().Contain("Wiedźmin");
        processed.Text.Should().Contain("Geralt z Rivii");
        processed.Text.Should().Contain("Ostatnie Życzenie");
        processed.Text.Should().Contain("Jaskier");
        processed.Text.Should().NotContain("color: red");
        processed.Text.Should().NotContain("alert('x')");
    }

    [Test]
    public async Task ProcessFileAsync_ShouldExtractText_WhenMimeTypeIsPptx()
    {
        var slide1 = "<p:sld xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\"><p:cSld><p:spTree><p:sp><p:txBody><a:p><a:r><a:t>Wykład 1: Wprowadzenie</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:sld>";

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("ppt/slides/slide1.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(slide1);
        }

        ms.Position = 0;
        var aiFile = new AIFile
        {
            Stream = ms,
            MimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
        };

        var processed = await _service.ProcessFileAsync(aiFile);

        processed.Should().NotBeNull();
        processed!.IsTextFormat.Should().BeTrue();
        processed.Text.Should().Contain("Wykład 1: Wprowadzenie");
    }

    [Test]
    public async Task ProcessFileAsync_ShouldExtractText_WhenMimeTypeIsRtf()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl {\\f0 Arial;}} \\f0\\fs24 Treść dokumentu RTF z polskimi znakami.}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(rtf));
        var aiFile = new AIFile
        {
            Stream = stream,
            MimeType = "application/rtf"
        };

        var processed = await _service.ProcessFileAsync(aiFile);

        processed.Should().NotBeNull();
        processed!.IsTextFormat.Should().BeTrue();
        processed.Text.Should().Contain("Treść dokumentu RTF");
    }
}
