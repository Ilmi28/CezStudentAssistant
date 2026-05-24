using CezStudentAssistant.Infrastructure.Services;
using FluentAssertions;

namespace CezStudentAssistant.UnitTests.Infrastructure.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _sut;

    public PasswordServiceTests()
    {
        _sut = new PasswordService();
    }

    [Test]
    public void CreatePasswordHash_ShouldReturnHash_WhenPasswordProvided()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = _sut.CreatePasswordHash(password);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotBe(password);
    }

    [Test]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _sut.CreatePasswordHash(password);

        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword";
        var hash = _sut.CreatePasswordHash(password);

        // Act
        var result = _sut.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }
}
