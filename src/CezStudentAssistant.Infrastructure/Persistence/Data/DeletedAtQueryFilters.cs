using CezStudentAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace CezStudentAssistant.Infrastructure.Persistence.Data;

internal static class DeletedAtQueryFilters
{
    internal static void ApplyDeletedAtFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var deletedAtProperty = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var deletedAtIsNull = Expression.Equal(
                deletedAtProperty,
                Expression.Constant(null, typeof(DateTime?)));

            var filter = Expression.Lambda(deletedAtIsNull, parameter);
            entityType.SetQueryFilter(filter);
        }
    }
}
