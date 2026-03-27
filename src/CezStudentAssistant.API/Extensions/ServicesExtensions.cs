using CezStudentAssistant.API.Decorators;
using CezStudentAssistant.Domain.Interfaces.Common;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using CezStudentAssistant.Infrastructure.Persistence.Repositories;
using CezStudentAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.API.Extensions;

public static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCqrsHandlers()
        {
            services.Scan(scan =>
                scan.FromAssembliesOf(typeof(IQueryHandler<,>))
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            return services;
        }

        public IServiceCollection AddSqlServer(
            bool loggingEnabled = false,
            bool detailedErrors = false
        )
        {
            var connectionString = Environment.GetEnvironmentVariable(
                "SQLSERVER_CONNECTION_STRING"
            );

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);

                if (loggingEnabled)
                    options.EnableSensitiveDataLogging();
                if (detailedErrors)
                    options.EnableDetailedErrors();
            });

            return services;
        }

        public IServiceCollection AddRepositories()
        {
            services.Scan(scan =>
                scan.FromAssembliesOf(typeof(ExampleEntityRepository))
                    .AddClasses(classes => classes.AssignableTo(typeof(IGenericRepository<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public IServiceCollection AddServices()
        {
            services.Scan(scan =>
                scan.FromAssembliesOf(typeof(CurrentUserService))
                    .AddClasses(classes => classes.AssignableTo(typeof(IScopedService)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            services.Scan(scan =>
                scan.FromAssembliesOf(typeof(PasswordService))
                    .AddClasses(classes => classes.AssignableTo(typeof(ISingletonService)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
            return services;
        }

        public IServiceCollection AddLoggingDecorator()
        {
            services.Decorate(
                typeof(ICommandHandler<,>),
                typeof(LoggingDecorator.CommandHandlerDecorator<,>)
            );
            services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandlerDecorator<>));
            services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandlerDecorator<,>));
            return services;
        }
    }
}
