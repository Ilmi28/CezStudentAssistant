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
        var password = "TestPassword123!";

        var result = _sut.CreatePasswordHash(password);

        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotBe(password);
    }

    [Test]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var password = "TestPassword123!";
        var hash = _sut.CreatePasswordHash(password);

        var result = _sut.VerifyPassword(password, hash);

        result.Should().BeTrue();
    }

    [Test]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword";
        var hash = _sut.CreatePasswordHash(password);

        var result = _sut.VerifyPassword(wrongPassword, hash);

        result.Should().BeFalse();
    }
}
