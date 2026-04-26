using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Common;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInstrastructure(bool isDevelopment)
        {
            return services
                .AddServices()
                .AddSqlServer(loggingEnabled: isDevelopment, detailedErrors: isDevelopment)
                .AddRepositories();
        }
        private IServiceCollection AddSqlServer(
            bool loggingEnabled = false,
            bool detailedErrors = false
        )
        {
            var connectionString = Environment.GetEnvironmentVariable(
                "DB_CONNECTION_STRING"
            );

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

        private IServiceCollection AddRepositories()
        {
            var asesmblies = AppDomain.CurrentDomain.GetAssemblies();
            services.Scan(scan =>
                scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(classes => classes.AssignableTo(typeof(IGenericRepository<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        private IServiceCollection AddServices()
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
}
