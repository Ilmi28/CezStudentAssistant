using AutoMapper;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez;
using CezStudentAssistant.Cez.Responses;
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
        result.ExternalId.Should().Be(1);
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

    [Test]
    public void ExternalCezCourseSection_List_To_CezCourseContentResponse_ShouldFlattenModulesAndContents()
    {
        // Arrange
        var source = new CezRequestResult<List<ExternalCezCourseSection>>
        {
            Data =
            [
                new ExternalCezCourseSection
                {
                    Modules =
                    [
                        new ExternalModule
                        {
                            Id = 10,
                            Contents =
                            [
                                new ExternalContent { Id = 100, Type = "file", FileUrl = "https://example.com/1" },
                                new ExternalContent { Id = 101, Type = "page", FileUrl = "https://example.com/2" }
                            ]
                        }
                    ]
                }
            ]
        };

        // Act
        var result = _mapper.Map<CezCourseContentResponse>(source);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data![0].Id.Should().Be(100);
        result.Data[0].Type.Should().Be("file");
        result.Data[0].FileUrl.Should().Be("https://example.com/1");
        result.Data[0].ModuleId.Should().Be(10);
        result.Data[1].Id.Should().Be(101);
        result.Data[1].Type.Should().Be("page");
        result.Data[1].FileUrl.Should().Be("https://example.com/2");
        result.Data[1].ModuleId.Should().Be(10);
    }
}
