using CezStudentAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CezStudentAssistant.Infrastructure.Persistence.Configurations;

public class FlashcardAttemptCardConfiguration : IEntityTypeConfiguration<FlashcardAttemptCard>
{
    public void Configure(EntityTypeBuilder<FlashcardAttemptCard> builder)
    {
        builder.ToTable("FlashcardAttemptCards");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.FlashcardAttempt)
            .WithMany(a => a.Cards)
            .HasForeignKey(x => x.FlashcardAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Flashcard)
            .WithMany()
            .HasForeignKey(x => x.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
