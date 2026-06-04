using CezStudentAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CezStudentAssistant.Infrastructure.Persistence.Data;

internal static class DeletedAtQueryFilters
{
    internal static void ApplyDeletedAtFilters(this ModelBuilder modelBuilder)
    {
        var applyFilterMethod = typeof(DeletedAtQueryFilters)
            .GetMethod(nameof(ApplyFilter), BindingFlags.NonPublic | BindingFlags.Static);

        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            applyFilterMethod?
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, new object[] { modelBuilder });
        }
    }

    private static void ApplyFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.DeletedAt == null);
    }
}
