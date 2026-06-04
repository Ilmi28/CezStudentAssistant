using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.UnitTests.Infrastructure.Persistence.Data;

public class AppDbContextTests
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
    public async Task SaveChangesAsync_ShouldSetAuditProperties_WhenAddingEntity()
    {
        // Arrange
        var user = new User { UserName = "new_user" };
        _context.Users.Add(user);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.LastModifiedAt.Should().BeCloseTo(user.CreatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Test]
    public async Task SaveChangesAsync_ShouldUpdateLastModifiedAt_WhenModifyingEntity()
    {
        // Arrange
        var user = new User { UserName = "user" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var originalLastModified = user.LastModifiedAt;
        await Task.Delay(10); // Ensure a small time gap

        // Act
        user.UserName = "updated_user";
        await _context.SaveChangesAsync();

        // Assert
        user.LastModifiedAt.Should().BeAfter(originalLastModified);
        user.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPerformSoftDelete_WhenEntityIsDeleted()
    {
        // Arrange
        var user = new User { UserName = "to_delete" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        _context.Users.Remove(user);
        
        // Before SaveChanges, state should be Deleted
        _context.Entry(user).State.Should().Be(EntityState.Deleted);

        await _context.SaveChangesAsync();

        // Assert
        // State should have been changed to Modified during SaveChanges
        _context.Entry(user).State.Should().Be(EntityState.Unchanged); 
        
        user.DeletedAt.Should().NotBeNull();
        user.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.LastModifiedAt.Should().BeCloseTo(user.DeletedAt.Value, TimeSpan.FromMilliseconds(100));
    }

    [Test]
    public async Task SaveChangesAsync_ShouldKeepExistingCreatedAt_WhenModifyingEntity()
    {
        // Arrange
        var user = new User { UserName = "user" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var originalCreatedAt = user.CreatedAt;

        // Act
        user.UserName = "updated";
        await _context.SaveChangesAsync();

        // Assert
        user.CreatedAt.Should().Be(originalCreatedAt);
    }
}
