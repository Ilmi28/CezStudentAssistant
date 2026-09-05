using CezStudentAssistant.Application.Commands.Course;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserEntity = CezStudentAssistant.Domain.Entities.User;
using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

namespace CezStudentAssistant.UnitTests.Application.Commands.Course;

[TestFixture]
public class ToggleCourseFileVisibilityCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private ToggleCourseFileVisibilityCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);

        _sut = new ToggleCourseFileVisibilityCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldToggleIsHiddenFromFalseToTrue_WhenResourceIsVisible()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "user" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Physics",
            Type = CourseType.User,
            Users = new List<UserEntity> { user }
        };
        var resource = new Resource
        {
            Id = fileId,
            CourseId = courseId,
            Name = "lecture.pdf",
            DisplayName = "lecture.pdf",
            MimeType = "application/pdf",
            IsHidden = false
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);
        _resourceRepository.GetByIdAsync(fileId, Arg.Any<CancellationToken>(), false)
            .Returns(resource);

        var command = new ToggleCourseFileVisibilityCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileId = fileId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsHidden.Should().BeTrue();
        resource.IsHidden.Should().BeTrue();

        await _resourceRepository.Received(1).UpdateAsync(resource, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldToggleIsHiddenFromTrueToFalse_WhenResourceIsHidden()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "user" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Physics",
            Type = CourseType.User,
            Users = new List<UserEntity> { user }
        };
        var resource = new Resource
        {
            Id = fileId,
            CourseId = courseId,
            Name = "lecture.pdf",
            DisplayName = "lecture.pdf",
            MimeType = "application/pdf",
            IsHidden = true
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);
        _resourceRepository.GetByIdAsync(fileId, Arg.Any<CancellationToken>(), false)
            .Returns(resource);

        var command = new ToggleCourseFileVisibilityCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileId = fileId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsHidden.Should().BeFalse();
        resource.IsHidden.Should().BeFalse();

        await _resourceRepository.Received(1).UpdateAsync(resource, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns((CourseEntity?)null);

        var command = new ToggleCourseFileVisibilityCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileId = fileId
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(CourseMessageConsts.CourseNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserHasNoAccess()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var otherUser = new UserEntity { Id = Guid.NewGuid(), UserName = "other" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Physics",
            Type = CourseType.User,
            Users = new List<UserEntity> { otherUser }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var command = new ToggleCourseFileVisibilityCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileId = fileId
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(CourseMessageConsts.CourseAccessDenied);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenFileDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
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
        _resourceRepository.GetByIdAsync(fileId, Arg.Any<CancellationToken>(), false)
            .Returns((Resource?)null);

        var command = new ToggleCourseFileVisibilityCommand
        {
            UserId = userId,
            CourseId = courseId,
            FileId = fileId
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(GeneralMessageConsts.FileNotFound);
    }
}
