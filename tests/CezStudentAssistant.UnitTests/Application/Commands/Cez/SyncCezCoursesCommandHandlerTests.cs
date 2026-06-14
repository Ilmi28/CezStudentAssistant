using CezStudentAssistant.Application.Commands.Cez;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using NSubstitute;

namespace CezStudentAssistant.UnitTests.Application.Commands.Cez;

public class SyncCezCoursesCommandHandlerTests
{
    private ICezService _cezService = null!;
    private SyncCezCoursesCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezService = Substitute.For<ICezService>();
        _sut = new SyncCezCoursesCommandHandler(_cezService);
    }

    [Test]
    public async Task Handle_ShouldCallCezServiceSyncUserCourses_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SyncCezCoursesCommand { UserId = userId };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
        await _cezService.Received(1).SyncUserCourses(userId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowException_WhenCezServiceThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SyncCezCoursesCommand { UserId = userId };
        _cezService.When(x => x.SyncUserCourses(userId, Arg.Any<CancellationToken>()))
            .Do(x => throw new Exception("API Error"));

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
