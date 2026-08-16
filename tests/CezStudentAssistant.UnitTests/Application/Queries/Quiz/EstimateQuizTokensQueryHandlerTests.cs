using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.Quiz;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Queries.Quiz;

using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

[TestFixture]
public class EstimateQuizTokensQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private ITokenUsageRepository _tokenUsageRepository = null!;
    private IConfiguration _configuration = null!;
    private EstimateQuizTokensQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _courseRepository = Substitute.For<ICourseRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();
        _tokenUsageRepository = Substitute.For<ITokenUsageRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);
        _unitOfWork.Repository<ITokenUsageRepository>().Returns(_tokenUsageRepository);

        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Gemini:MaximumDailyTokens", "1000000" }
        }).Build();

        _sut = new EstimateQuizTokensQueryHandler(_unitOfWork, _configuration);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnEstimatedTokensAndPercentage_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var course = new CourseEntity { Id = courseId, Name = "Test Course", Type = Domain.Enums.CourseType.Cez };
        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = "slides.pdf",
            DisplayName = "Slides",
            MimeType = "application/pdf",
            EstimatedTokens = 10000,
            CourseId = courseId
        };
        var mockResourceDbSet = new List<Resource> { resource }.BuildMockDbSet();
        _resourceRepository.Find(Arg.Any<Expression<Func<Resource, bool>>>()).Returns(mockResourceDbSet);

        _tokenUsageRepository.GetDailyTokenUsageAsync(userId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(50000);

        var query = new EstimateQuizTokensQuery
        {
            UserId = userId,
            CourseId = courseId,
            QuestionCount = 5,
            AdditionalInstructions = "Focus on basics"
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.EstimatedTokens.Should().Be(11000);
        result.Data.DailyTokenLimit.Should().Be(1000000);
        result.Data.DailyTokensUsed.Should().Be(50000);
        result.Data.EstimatedDailyUsagePercentage.Should().Be(1.1);
        result.Data.RemainingDailyTokens.Should().Be(950000);
        result.Data.CanGenerate.Should().BeTrue();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var courseId = Guid.NewGuid();
        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns((CourseEntity?)null);

        var query = new EstimateQuizTokensQuery
        {
            UserId = Guid.NewGuid(),
            CourseId = courseId,
            QuestionCount = 5
        };

        Func<Task> act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(AIMessageConsts.CourseNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenMaximumDailyTokensConfigMissing()
    {
        var invalidConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        var sut = new EstimateQuizTokensQueryHandler(_unitOfWork, invalidConfig);

        var courseId = Guid.NewGuid();
        var course = new CourseEntity { Id = courseId, Name = "Test Course", Type = Domain.Enums.CourseType.Cez };
        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var query = new EstimateQuizTokensQuery
        {
            UserId = Guid.NewGuid(),
            CourseId = courseId,
            QuestionCount = 5
        };

        Func<Task> act = () => sut.Handle(query, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<AppException>();
        exceptionAssertion.WithInnerException<InvalidOperationException>()
            .WithMessage(UserMessageConsts.MaximumDailyTokensConfigMissing);
    }

    [Test]
    public async Task Handle_ShouldReturnCanGenerateFalse_WhenTokensExceedDailyLimit()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var course = new CourseEntity { Id = courseId, Name = "Test Course", Type = Domain.Enums.CourseType.Cez };
        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = "book.pdf",
            DisplayName = "Book",
            MimeType = "application/pdf",
            EstimatedTokens = 5000,
            CourseId = courseId
        };
        var mockResourceDbSet = new List<Resource> { resource }.BuildMockDbSet();
        _resourceRepository.Find(Arg.Any<Expression<Func<Resource, bool>>>()).Returns(mockResourceDbSet);

        _tokenUsageRepository.GetDailyTokenUsageAsync(userId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(995000);

        var query = new EstimateQuizTokensQuery
        {
            UserId = userId,
            CourseId = courseId,
            QuestionCount = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data!.EstimatedTokens.Should().Be(7000);
        result.Data.CanGenerate.Should().BeFalse();
    }
}
