using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.User;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Queries.User;

[TestFixture]
public class GetUserUsageQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private ITokenUsageRepository _tokenUsageRepository = null!;
    private IConfiguration _configuration = null!;
    private GetUserUsageQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _tokenUsageRepository = Substitute.For<ITokenUsageRepository>();
        _configuration = Substitute.For<IConfiguration>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<ITokenUsageRepository>().Returns(_tokenUsageRepository);

        _sut = new GetUserUsageQueryHandler(_unitOfWork, _configuration);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnUsageAndPercentage_WhenUserExistsAndTokensUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUsageQuery { UserId = userId };

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<Domain.Entities.User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _configuration["Gemini:MaximumDailyTokens"].Returns("1000000");

        _tokenUsageRepository.GetDailyTokenUsageAsync(userId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(150000);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.GetUserUsageSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.DailyTokensUsed.Should().Be(150000);
        result.Data.DailyTokenLimit.Should().Be(1000000);
        result.Data.DailyUsagePercentage.Should().Be(15.0);
    }

    [Test]
    public async Task Handle_ShouldReturnZeroUsage_WhenUserHasNoTokenUsageToday()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUsageQuery { UserId = userId };

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<Domain.Entities.User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _configuration["Gemini:MaximumDailyTokens"].Returns("1000000");

        _tokenUsageRepository.GetDailyTokenUsageAsync(userId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DailyTokensUsed.Should().Be(0);
        result.Data.DailyTokenLimit.Should().Be(1000000);
        result.Data.DailyUsagePercentage.Should().Be(0.0);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUsageQuery { UserId = userId };

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<Domain.Entities.User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(UserMessageConsts.UserNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenMaximumDailyTokensConfigMissing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUsageQuery { UserId = userId };

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<Domain.Entities.User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _configuration["Gemini:MaximumDailyTokens"].Returns((string?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(UserMessageConsts.MaximumDailyTokensConfigMissing);
    }
}
