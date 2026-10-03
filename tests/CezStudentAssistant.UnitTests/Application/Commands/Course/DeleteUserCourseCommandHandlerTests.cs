using CezStudentAssistant.Application.Commands.Course;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
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
public class DeleteUserCourseCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private ICascadeDeleteService _cascadeDeleteService = null!;
    private DeleteUserCourseCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _cascadeDeleteService = Substitute.For<ICascadeDeleteService>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _sut = new DeleteUserCourseCommandHandler(_unitOfWork, _cascadeDeleteService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldDeleteCourse_WhenValidUserCourse()
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

        var command = new DeleteUserCourseCommand { UserId = userId, CourseId = courseId };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        await _cascadeDeleteService.Received(1).DeleteCourseCascadeAsync(course, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns((CourseEntity?)null);

        var command = new DeleteUserCourseCommand { UserId = userId, CourseId = courseId };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(CourseMessageConsts.CourseNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenCourseIsCezType()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var course = new CourseEntity { Id = courseId, Name = "CEZ Course", Type = CourseType.Cez };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var command = new DeleteUserCourseCommand { UserId = userId, CourseId = courseId };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(CourseMessageConsts.CourseInvalidType);
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserDoesNotBelongToCourse()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var otherUser = new UserEntity { Id = otherUserId, UserName = "other" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Course",
            Type = CourseType.User,
            Users = new List<UserEntity> { otherUser }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), false, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var command = new DeleteUserCourseCommand { UserId = userId, CourseId = courseId };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(CourseMessageConsts.CourseAccessDenied);
    }
}
