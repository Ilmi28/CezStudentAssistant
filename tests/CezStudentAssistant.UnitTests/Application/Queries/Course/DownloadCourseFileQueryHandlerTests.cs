using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Queries.Course;
using CezStudentAssistant.Domain.Entities;
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

namespace CezStudentAssistant.UnitTests.Application.Queries.Course;

[TestFixture]
public class DownloadCourseFileQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private IFileService _fileService = null!;
    private DownloadCourseFileQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();
        _fileService = Substitute.For<IFileService>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "BlobContainerSettings:CourseFilesContainer", "course-files" } }).Build();
        _sut = new DownloadCourseFileQueryHandler(_unitOfWork, _fileService, configuration);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnFileStream_WhenValidCourseAndResource()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "user" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Course",
            Users = new List<UserEntity> { user }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var resource = new Resource
        {
            Id = fileId,
            CourseId = courseId,
            Name = "guid_file.pdf",
            DisplayName = "file.pdf",
            MimeType = "application/pdf"
        };

        _resourceRepository.GetByIdAsync(fileId, Arg.Any<CancellationToken>(), true)
            .Returns(resource);

        using var memoryStream = new MemoryStream(new byte[] { 10, 20, 30 });
        _fileService.DownloadAsync($"{courseId}/guid_file.pdf", "course-files", Arg.Any<CancellationToken>())
            .Returns(memoryStream);

        var query = new DownloadCourseFileQuery { UserId = userId, CourseId = courseId, FileId = fileId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.FileName.Should().Be("file.pdf");
        result.ContentType.Should().Be("application/pdf");
        result.FileStream.Should().BeSameAs(memoryStream);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns((CourseEntity?)null);

        var query = new DownloadCourseFileQuery { UserId = userId, CourseId = courseId, FileId = fileId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(CourseMessageConsts.CourseNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserDoesNotBelongToCourse()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var otherUser = new UserEntity { Id = otherUserId, UserName = "other" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Course A",
            Users = new List<UserEntity> { otherUser }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var query = new DownloadCourseFileQuery { UserId = userId, CourseId = courseId, FileId = fileId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(CourseMessageConsts.CourseAccessDenied);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "user" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Course B",
            Users = new List<UserEntity> { user }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        _resourceRepository.GetByIdAsync(fileId, Arg.Any<CancellationToken>(), true)
            .Returns((Resource?)null);

        var query = new DownloadCourseFileQuery { UserId = userId, CourseId = courseId, FileId = fileId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(GeneralMessageConsts.FileNotFound);
    }
}
