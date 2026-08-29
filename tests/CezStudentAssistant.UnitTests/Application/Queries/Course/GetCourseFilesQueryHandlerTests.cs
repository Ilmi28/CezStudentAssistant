using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.Course;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserEntity = CezStudentAssistant.Domain.Entities.User;
using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

namespace CezStudentAssistant.UnitTests.Application.Queries.Course;

[TestFixture]
public class GetCourseFilesQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private GetCourseFilesQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);

        _sut = new GetCourseFilesQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnCourseFiles_WhenCourseExistsAndBelongsToUser()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "student" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Operating Systems",
            Users = new List<UserEntity> { user }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Name = "syllabus.pdf",
            DisplayName = "syllabus.pdf",
            MimeType = "application/pdf"
        };
        var mockResources = new List<Resource> { resource }.BuildMockDbSet();
        _resourceRepository.Find(Arg.Any<Expression<Func<Resource, bool>>>(), true).Returns(mockResources);

        var query = new GetCourseFilesQuery { UserId = userId, CourseId = courseId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].DisplayName.Should().Be("syllabus.pdf");
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns((CourseEntity?)null);

        var query = new GetCourseFilesQuery { UserId = userId, CourseId = courseId };

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
        var otherUser = new UserEntity { Id = otherUserId, UserName = "other" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "OS",
            Users = new List<UserEntity> { otherUser }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var query = new GetCourseFilesQuery { UserId = userId, CourseId = courseId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(CourseMessageConsts.CourseAccessDenied);
    }
}
