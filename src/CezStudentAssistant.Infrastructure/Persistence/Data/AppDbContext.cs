using CezStudentAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Infrastructure.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyDeletedAtFilters();
        modelBuilder.Entity<Quiz>().ToTable("Quiz");
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            var baseEntity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    baseEntity.CreatedAt = DateTime.UtcNow;
                    baseEntity.LastModifiedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    baseEntity.LastModifiedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    baseEntity.LastModifiedAt = DateTime.UtcNow;
                    baseEntity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(ct);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<CezUser> CezUsers { get; set; }
    public DbSet<UserConfiguration> UserConfigurations { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Job> Jobs { get; set; }
}
