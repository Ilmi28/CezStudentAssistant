using CezStudentAssistant.Application.Commands.Cez;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Cez;

[TestFixture]
public class DisconnectCezCommandHandlerTests
{
    private ICezService _cezService = null!;
    private DisconnectCezCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezService = Substitute.For<ICezService>();
        _sut = new DisconnectCezCommandHandler(_cezService);
    }

    [Test]
    public async Task Handle_ShouldCallDisconnectCezAsync_WhenCommandIsValid()
    {
        var userId = Guid.NewGuid();
        var command = new DisconnectCezCommand { UserId = userId };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();

        await _cezService.Received(1).DisconnectCezAsync(userId, Arg.Any<CancellationToken>());
    }
}
