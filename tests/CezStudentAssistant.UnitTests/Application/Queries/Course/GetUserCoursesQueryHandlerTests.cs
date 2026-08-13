using CezStudentAssistant.Application.Queries.Course;
using CezStudentAssistant.Application.Interfaces.Persistence;
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
public class GetUserCoursesQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private GetUserCoursesQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _sut = new GetUserCoursesQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnUserCourses_WhenCoursesExistForUser()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "student" };
        var courseCez = new CourseEntity
        {
            Id = Guid.NewGuid(),
            Name = "Data Structures",
            CezExternalId = 12345,
            Users = new List<UserEntity> { user }
        };
        var courseCustom = new CourseEntity
        {
            Id = Guid.NewGuid(),
            Name = "My Custom Course",
            CezExternalId = null,
            Users = new List<UserEntity> { user }
        };

        var mockDbSet = new List<CourseEntity> { courseCez, courseCustom }.BuildMockDbSet();
        _courseRepository.Find(Arg.Any<Expression<Func<CourseEntity, bool>>>()).Returns(mockDbSet);

        var query = new GetUserCoursesQuery { UserId = userId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].IsCez.Should().BeTrue();
        result.Data![1].IsCez.Should().BeFalse();
    }
}
