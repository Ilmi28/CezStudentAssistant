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
        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.ToTable("Quiz");
            entity.HasOne(q => q.User)
                  .WithMany(u => u.Quizzes)
                  .HasForeignKey(q => q.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ChatThread>(entity =>
        {
            entity.HasOne(t => t.User)
                  .WithMany(u => u.ChatThreads)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Course)
                  .WithMany(c => c.ChatThreads)
                  .HasForeignKey(t => t.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.AttachedResources)
                  .WithMany()
                  .UsingEntity("ChatThreadResources");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(m => m.ChatThread)
                  .WithMany(t => t.Messages)
                  .HasForeignKey(m => m.ChatThreadId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
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
    public DbSet<TokenUsage> TokenUsages { get; set; }
    public DbSet<FlashcardDeck> FlashcardDecks { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<FlashcardAttempt> FlashcardAttempts { get; set; }
    public DbSet<FlashcardAttemptCard> FlashcardAttemptCards { get; set; }
    public DbSet<ChatThread> ChatThreads { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
}
