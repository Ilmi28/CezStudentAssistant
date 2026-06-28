using Azure.Storage.Blobs;
using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInstrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        return services
            .AddServices()
            .AddDatabase(configuration, loggingEnabled: isDevelopment, detailedErrors: isDevelopment)
            .AddRepositories()
            .AddHangfireConfiguration(configuration)
            .AddAzureBlobStorage(configuration);
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        bool loggingEnabled = false,
        bool detailedErrors = false
    )
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

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

            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
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

    private static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(connectionString);
            }));

        return services;
    }

    private static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");

        services.AddSingleton(x => new BlobServiceClient(connectionString));

        return services;
    }
}
