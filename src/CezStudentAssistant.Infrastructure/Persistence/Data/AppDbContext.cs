using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using CezStudentAssistant.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Infrastructure.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
    : DbContext(options)
{
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
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
                        baseEntity.DeletedAt = DateTime.UtcNow;
                        break;
                }
            }

            if (entry.Entity is IAuditableEntity auditable && entry.State == EntityState.Added)
            {
                auditable.UserId = currentUserService.UserId;
            }
        }

        return base.SaveChangesAsync(ct);
    }

    public DbSet<ExampleEntity> ExampleEntities { get; set; }

    public DbSet<User> Users { get; set; }
}
