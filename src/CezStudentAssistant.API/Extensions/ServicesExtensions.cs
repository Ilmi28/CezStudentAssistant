using CezStudentAssistant.API.Decorators;
using CezStudentAssistant.API.Interfaces.CQRS;

namespace CezStudentAssistant.API.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddCQRSHandlers(this IServiceCollection services)
        {
            services.Scan(scan => scan.FromAssembliesOf(typeof(Program))
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            return services;
        }

        public static IServiceCollection AddLoggingDecorator(this IServiceCollection services)
        {
            services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
            services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
            return services;
        }
    }
}
