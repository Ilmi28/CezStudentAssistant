using CezStudentAssistant.Application.Commands.Cez;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Notifications;
using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Cez;

[TestFixture]
public class ConnectCezCommandHandlerTests
{
    private ICezService _cezService = null!;
    private IPublisher _publisher = null!;
    private ConnectCezCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezService = Substitute.For<ICezService>();
        _publisher = Substitute.For<IPublisher>();
        _sut = new ConnectCezCommandHandler(_cezService, _publisher);
    }

    [Test]
    public async Task Handle_ShouldCallConnectCezAsyncAndPublishNotification_WhenCommandIsValid()
    {
        var userId = Guid.NewGuid();
        var command = new ConnectCezCommand("cezuser", "password") { UserId = userId };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();

        await _cezService.Received(1).ConnectCezAsync(userId, "cezuser", "password", Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(Arg.Is<CezLoginSucceededNotification>(n => n.UserId == userId), Arg.Any<CancellationToken>());
    }
}
