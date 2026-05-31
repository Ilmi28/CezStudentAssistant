using CezStudentAssistant.Application.Decorators;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Services;
using CezStudentAssistant.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            return services
                .AddCqrsHandlers()
                .AddValidators()
                .AddServices()
                .AddLoggingDecorator();
        }

        private IServiceCollection AddValidators()
        {
            services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
            return services;
        }

        private IServiceCollection AddCqrsHandlers()
        {
            static bool IsDecorator(Type type) =>
                type == typeof(LoggingDecorator.CommandHandlerDecorator<,>) ||
                type == typeof(LoggingDecorator.CommandHandlerDecorator<>) ||
                type == typeof(LoggingDecorator.QueryHandlerDecorator<,>);

            services.Scan(scan =>
                scan.FromAssembliesOf(typeof(IQueryHandler<,>))
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)).Where(type => !IsDecorator(type)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)).Where(type => !IsDecorator(type)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)).Where(type => !IsDecorator(type)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            return services;
        }

        private IServiceCollection AddLoggingDecorator()
        {
            //services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandlerDecorator<,>));
            services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandlerDecorator<>));
            //services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandlerDecorator<,>));
            return services;
        }

        private IServiceCollection AddServices()
        {
            services.AddScoped<ICezService, CezService>();
            return services;
        }
    }
}
