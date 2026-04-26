using CezStudentAssistant.Application.Decorators;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Validators;
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
                .AddLoggingDecorator();
        }

        private IServiceCollection AddValidators()
        {
            services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
            return services;
        }

        private IServiceCollection AddCqrsHandlers()
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

        private IServiceCollection AddLoggingDecorator()
        {
            services.Decorate(
                typeof(ICommandHandler<,>),
                typeof(LoggingDecorator.CommandHandlerDecorator<,>)
            );
            //services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandlerDecorator<>));
            //services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandlerDecorator<,>));
            return services;
        }
    }
}
