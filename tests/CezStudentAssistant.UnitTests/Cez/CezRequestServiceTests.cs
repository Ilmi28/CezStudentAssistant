using System.Net;
using CezStudentAssistant.Cez.Responses;
using CezStudentAssistant.Cez.Services;
using FluentAssertions;

namespace CezStudentAssistant.UnitTests.Cez;

public class CezRequestServiceTests
{
    private TestHttpMessageHandler _httpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private CezRequestService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandler = new TestHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandler)
        {
            BaseAddress = new Uri("https://test.com/")
        };
        _sut = new CezRequestService(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
        _httpMessageHandler.Dispose();
    }

    [Test]
    public async Task SendGetAsync_ShouldReturnData_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedData = new TestData { Name = "Test" };
        var jsonResponse = "{\"name\": \"Test\"}";
        
        _httpMessageHandler.Sender = (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        });

        // Act
        var result = await _sut.SendGetAsync<TestData>("path", []);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test");
    }

    [Test]
    public async Task SendGetAsync_ShouldReturnData_WhenResponseIsArray()
    {
        // Arrange
        var jsonResponse = "[{\"name\": \"Test1\"}, {\"name\": \"Test2\"}]";
        
        _httpMessageHandler.Sender = (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        });

        // Act
        var result = await _sut.SendGetAsync<List<TestData>>("path", []);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data![0].Name.Should().Be("Test1");
        result.Data![1].Name.Should().Be("Test2");
    }

    [Test]
    public async Task SendGetAsync_ShouldReturnError_WhenResponseContainsCezError()
    {
        // Arrange
        var jsonResponse = "{\"errorcode\": \"invalidlogin\", \"message\": \"Invalid credentials\"}";
        
        _httpMessageHandler.Sender = (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        });

        // Act
        var result = await _sut.SendGetAsync<TestData>("path", []);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("invalidlogin");
        result.Error!.Message.Should().Be("Invalid credentials");
    }

    [Test]
    public async Task SendGetAsync_ShouldExtractJson_WhenResponseIsWrapped()
    {
        // Arrange
        var wrappedResponse = "Non-JSON prefix {\"name\": \"Test\"} Non-JSON suffix";
        
        _httpMessageHandler.Sender = (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(wrappedResponse)
        });

        // Act
        var result = await _sut.SendGetAsync<TestData>("path", []);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test");
    }

    [Test]
    public async Task SendGetAsync_ShouldCorrectlyEncodeQueryParams()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        _httpMessageHandler.Sender = (req, ct) => 
        {
            capturedRequest = req;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        };

        var queryParams = new List<KeyValuePair<string, string>>
        {
            new("param1", "value1"),
            new("param2", "value with spaces")
        };

        // Act
        await _sut.SendGetAsync<TestData>("path", queryParams);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.Query.Should().Be("?param1=value1&param2=value%20with%20spaces");
    }

    private class TestData
    {
        public string Name { get; set; } = null!;
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Sender { get; set; } = null!;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Sender(request, cancellationToken);
        }
    }
}
