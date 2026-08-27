using CezStudentAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CezStudentAssistant.Infrastructure.Persistence.Configurations;

public class FlashcardAttemptConfiguration : IEntityTypeConfiguration<FlashcardAttempt>
{
    public void Configure(EntityTypeBuilder<FlashcardAttempt> builder)
    {
        builder.ToTable("FlashcardAttempts");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Deck)
            .WithMany(d => d.Attempts)
            .HasForeignKey(x => x.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
