using AutoMapper;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez;
using CezStudentAssistant.Cez.Responses;
using CezStudentAssistant.Cez.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CezStudentAssistant.UnitTests.Cez;

public class CezProfileTests
{
    private IMapper _mapper = null!;
    private MapperConfiguration _config = null!;

    [SetUp]
    public void SetUp()
    {
        _config = new MapperConfiguration(cfg => cfg.AddProfile<CezProfile>(), NullLoggerFactory.Instance);
        _mapper = _config.CreateMapper();
    }

    [Test]
    public void Configuration_ShouldBeValid()
    {
        _config.AssertConfigurationIsValid();
    }

    [Test]
    public void ExternalCezGetUserCoursesResponse_To_CezCourse_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ExternalCezGetUserCoursesResponse { Id = 1, FullName = "Course Name" };

        // Act
        var result = _mapper.Map<CezCourse>(source);

        // Assert
        result.ExternalId.Should().Be("1");
        result.FullName.Should().Be("Course Name");
    }

    [Test]
    public void ExternalCezLoginResponse_To_CezTokens_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ExternalCezLoginResponse { Token = "token", PrivateToken = "private" };

        // Act
        var result = _mapper.Map<CezTokens>(source);

        // Assert
        result.Token.Should().Be("token");
        result.PrivateToken.Should().Be("private");
    }
}
