using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Common;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInstrastructure(this IServiceCollection services, bool isDevelopment)
    {
        return services
            .AddServices()
            .AddDatabase(loggingEnabled: isDevelopment, detailedErrors: isDevelopment)
            .AddRepositories();
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        bool loggingEnabled = false,
        bool detailedErrors = false
    )
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "DB_CONNECTION_STRING"
        );

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (loggingEnabled)
                options.EnableSensitiveDataLogging();
            if (detailedErrors)
                options.EnableDetailedErrors();
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(IGenericRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(IScopedService)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
        services.Scan(scan =>
            scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(ISingletonService)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );

        return services;
    }
}
