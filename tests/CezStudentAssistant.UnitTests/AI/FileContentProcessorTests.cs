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
    public async Task ProcessFileAsync_ShouldExtractText_WhenMimeTypeIsEpub()
    {
        var htmlContent = "<html><body><h1>Wiedźmin</h1><p>Rozdział 1: Pani Jeziora</p></body></html>";

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("OEBPS/Text/chapter1.xhtml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(htmlContent);
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
        processed.Text.Should().Contain("Pani Jeziora");
    }
}
