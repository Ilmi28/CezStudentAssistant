using CezStudentAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CezStudentAssistant.Infrastructure.Persistence.Configurations;

public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.ToTable("Flashcard");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Front)
            .IsRequired();

        builder.Property(x => x.Back)
            .IsRequired();

        builder.HasOne(x => x.Deck)
            .WithMany(d => d.Cards)
            .HasForeignKey(x => x.DeckId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
