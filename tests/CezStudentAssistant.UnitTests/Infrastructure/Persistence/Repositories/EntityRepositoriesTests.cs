using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using CezStudentAssistant.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.UnitTests.Infrastructure.Persistence.Repositories;

public class EntityRepositoriesTests
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
    public async Task CourseRepository_ShouldAddAndRetrieveEntity()
    {
        var repository = new CourseRepository(_context);
        var entity = new Course { Name = "Test Course" };

        await repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(entity.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Course");
    }

    [Test]
    public async Task QuestionRepository_ShouldAddAndRetrieveEntity()
    {
        var repository = new QuestionRepository(_context);
        var course = new Course { Name = "Course" };
        var entity = new Question 
        { 
            Content = "Test Question", 
            Course = course 
        };

        await repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(entity.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Content.Should().Be("Test Question");
    }

    [Test]
    public async Task QuestionOptionRepository_ShouldAddAndRetrieveEntity()
    {
        var repository = new QuestionOptionRepository(_context);
        var course = new Course { Name = "Course" };
        var question = new Question { Content = "Question", Course = course };
        var entity = new QuestionOption 
        { 
            Content = "Test Option", 
            Question = question 
        };

        await repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(entity.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Content.Should().Be("Test Option");
    }

    [Test]
    public async Task QuizAttemptRepository_ShouldAddAndRetrieveEntity()
    {
        var repository = new QuizAttemptRepository(_context);
        var user = new User { UserName = "testuser" };
        var course = new Course { Name = "Course" };
        var entity = new QuizAttempt 
        { 
            User = user,
            Course = course,
            Score = 10.5m 
        };

        await repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(entity.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Score.Should().Be(10.5m);
    }

    [Test]
    public async Task QuestionAnswerRepository_ShouldAddAndRetrieveEntity()
    {
        var repository = new QuestionAnswerRepository(_context);
        var user = new User { UserName = "testuser" };
        var course = new Course { Name = "Course" };
        var question = new Question { Content = "Question", Course = course };
        var quizAttempt = new QuizAttempt { User = user, Course = course };
        
        var entity = new QuestionAnswer 
        { 
            QuizAttempt = quizAttempt,
            Question = question,
            Answer = "My Answer"
        };

        await repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(entity.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Answer.Should().Be("My Answer");
    }
}
