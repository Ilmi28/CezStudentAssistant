using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.UnitTests.Infrastructure.Persistence.Data;

public class GenericRepositoryTests
{
    private AppDbContext _context = null!;
    private GenericRepository<User> _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new GenericRepository<User>(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_ShouldAddEntity()
    {
        var user = new User { UserName = "testuser" };

        await _sut.AddAsync(user);
        await _context.SaveChangesAsync();

        var addedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");
        addedUser.Should().NotBeNull();
        addedUser!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, UserName = "testuser" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.UserName.Should().Be("testuser");
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEntityWithIncludes()
    {
        var id = Guid.NewGuid();
        var user = new User 
        { 
            Id = id, 
            UserName = "testuser",
            RefreshTokens = new List<RefreshToken> { new() { Token = "token", ExpiryTime = DateTime.UtcNow.AddDays(1) } }
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(id, CancellationToken.None, u => u.RefreshTokens);

        result.Should().NotBeNull();
        result!.RefreshTokens.Should().NotBeEmpty();
        result.RefreshTokens.First().Token.Should().Be("token");
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        await _context.Users.AddRangeAsync(
            new User { UserName = "user1" },
            new User { UserName = "user2" }
        );
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        var user = new User { UserName = "oldname" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        user.UserName = "newname";
        await _sut.UpdateAsync(user);
        await _context.SaveChangesAsync();

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.UserName.Should().Be("newname");
        updatedUser.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        var user = new User { UserName = "testuser" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        await _sut.DeleteAsync(user);
        await _context.SaveChangesAsync();

        // Entity should still be in DB but with DeletedAt set
        var deletedUser = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
        deletedUser.Should().NotBeNull();
        deletedUser!.DeletedAt.Should().NotBeNull();
        
        // Query filter should hide it by default (assuming ApplyDeletedAtFilters works)
        var exists = await _context.Users.AnyAsync(u => u.Id == user.Id);
        exists.Should().BeFalse();
    }

    [Test]
    public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        var user = new User { UserName = "testuser" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.ExistsAsync(u => u.UserName == "testuser");

        result.Should().BeTrue();
    }

    [Test]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        await _context.Users.AddRangeAsync(
            new User { UserName = "match1" },
            new User { UserName = "match2" },
            new User { UserName = "other" }
        );
        await _context.SaveChangesAsync();

        var result = await _sut.FindAsync(u => u.UserName.StartsWith("match"));

        result.Should().HaveCount(2);
    }

    [Test]
    public async Task GetSingleAsync_ShouldReturnSingleEntity()
    {
        await _context.Users.AddAsync(new User { UserName = "testuser" });
        await _context.SaveChangesAsync();

        var result = await _sut.GetSingleAsync(u => u.UserName == "testuser");

        result.Should().NotBeNull();
        result!.UserName.Should().Be("testuser");
    }
}
