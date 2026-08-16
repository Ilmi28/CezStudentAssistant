using CezStudentAssistant.Application.Commands.Course;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserEntity = CezStudentAssistant.Domain.Entities.User;
using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

namespace CezStudentAssistant.UnitTests.Application.Commands.Course;

[TestFixture]
public class UploadCourseFileCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private IFileService _fileService = null!;
    private IAIClient _aiClient = null!;
    private UploadCourseFileCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();
        _fileService = Substitute.For<IFileService>();
        _aiClient = Substitute.For<IAIClient>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);
        _aiClient.EstimateTokenUsageAsync(Arg.Any<CezStudentAssistant.Application.Requests.AI.AIQuizRequest>()).Returns(1500);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "BlobContainerSettings:CourseFilesContainer", "course-files" } }).Build();
        _sut = new UploadCourseFileCommandHandler(_unitOfWork, _fileService, _aiClient, configuration);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldUploadFile_WhenValidUserCourse()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "user" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Physics",
            Type = CourseType.User,
            Users = new List<UserEntity> { user }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        using var memoryStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadCourseFileCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileName = "lecture.pdf",
            ContentType = "application/pdf",
            FileStream = memoryStream
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        await _fileService.Received(1).UploadAsync(
            Arg.Any<Stream>(),
            Arg.Is<string>(path => path.StartsWith($"{courseId}/")),
            "course-files",
            "application/pdf",
            Arg.Any<CancellationToken>()
        );
        await _resourceRepository.Received(1).AddAsync(Arg.Is<Resource>(r => r.DisplayName == "lecture.pdf" && r.CourseId == courseId && r.EstimatedTokens == 1500), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns((CourseEntity?)null);

        using var memoryStream = new MemoryStream();
        var command = new UploadCourseFileCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileName = "file.pdf",
            ContentType = "application/pdf",
            FileStream = memoryStream
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(CourseMessageConsts.CourseNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenFileFormatIsUnsupported()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        using var memoryStream = new MemoryStream();
        var command = new UploadCourseFileCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileName = "malware.exe",
            ContentType = "application/x-msdownload",
            FileStream = memoryStream
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(CourseMessageConsts.UnsupportedFileFormat);
    }
}
