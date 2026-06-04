using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.QueryHandlers;
using FluentAssertions;

namespace CezStudentAssistant.UnitTests.Application.QueryHandlers;

public class BaseQueryHandlerTests
{
    private TestQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new TestQueryHandler();
    }

    [Test]
    public async Task HandleAsync_ShouldReturnSuccessResponse_WhenExecutionIsSuccessful()
    {
        var query = new TestQuery();

        var result = await _sut.HandleAsync(query);

        result.Should().BeOfType<SuccessResponse<TestResponse>>();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Value.Should().Be("Result");
    }

    [Test]
    public async Task HandleAsync_ShouldThrowAppException_WhenExecutionFails()
    {
        var query = new TestQuery { ShouldFail = true };

        Func<Task> act = () => _sut.HandleAsync(query);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.WithMessage("Error");
        ex.WithInnerExceptionExactly<Exception>().WithMessage("Execution failed");
    }

    public class TestQuery : IQuery 
    {
        public bool ShouldFail { get; set; }
    }
    
    public class TestResponse 
    {
        public string Value { get; set; } = "Result";
    }

    private class TestQueryHandler : BaseQueryHandler<TestQuery, TestResponse>
    {
        protected override ApiMessage SuccessMessage => new(this, "Success");
        protected override ApiMessage ErrorMessage => new(this, "Error");

        protected override Task<TestResponse> ExecuteAsync(TestQuery query, CancellationToken ct)
        {
            if (query.ShouldFail)
                throw new Exception("Execution failed");

            return Task.FromResult(new TestResponse());
        }
    }
}
