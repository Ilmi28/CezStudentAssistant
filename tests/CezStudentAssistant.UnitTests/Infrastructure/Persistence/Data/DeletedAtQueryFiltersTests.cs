using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.UnitTests.Infrastructure.Persistence.Data;

public class DeletedAtQueryFiltersTests
{
    private AppDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task QueryFilters_ShouldHideDeletedEntities_ForAllBaseEntityTypes()
    {
        // Arrange - Create one of each main entity type
        var user = new User { UserName = "deleted_user" };
        var cezUser = new CezUser 
        { 
            ExternalUserId = 123, 
            FullName = "deleted_cez",
            Token = "token",
            PrivateToken = "private",
            User = user
        };
        var token = new RefreshToken { Token = "deleted_token", ExpiryTime = DateTime.UtcNow.AddDays(1) };

        await _context.Users.AddAsync(user);
        await _context.CezUsers.AddAsync(cezUser);
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act - Soft delete them manually
        user.DeletedAt = DateTime.UtcNow;
        cezUser.DeletedAt = DateTime.UtcNow;
        token.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert - They should be invisible to normal queries
        (await _context.Users.AnyAsync(u => u.Id == user.Id)).Should().BeFalse();
        (await _context.CezUsers.AnyAsync(u => u.Id == cezUser.Id)).Should().BeFalse();
        (await _context.RefreshTokens.AnyAsync(u => u.Id == token.Id)).Should().BeFalse();

        // Assert - They should be visible with IgnoreQueryFilters
        (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == user.Id)).Should().BeTrue();
        (await _context.CezUsers.IgnoreQueryFilters().AnyAsync(u => u.Id == cezUser.Id)).Should().BeTrue();
        (await _context.RefreshTokens.IgnoreQueryFilters().AnyAsync(u => u.Id == token.Id)).Should().BeTrue();
    }

    [Test]
    public async Task QueryFilters_ShouldShowActiveEntities()
    {
        // Arrange
        var user = new User { UserName = "active_user" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _context.Users.AnyAsync(u => u.Id == user.Id);

        // Assert
        exists.Should().BeTrue();
    }
}
