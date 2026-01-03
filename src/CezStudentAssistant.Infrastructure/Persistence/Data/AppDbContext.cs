using CezStudentAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Infrastructure.Persistence.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public AppDbContext() { }

    public DbSet<ExampleEntity> ExampleEntities { get; set; }
}
