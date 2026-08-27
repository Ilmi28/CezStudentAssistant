using CezStudentAssistant.Application.Behaviors;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Services;
using CezStudentAssistant.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddMediatRConfiguration()
            .AddValidators()
            .AddServices();
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
        return services;
    }

    private static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UserContextBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICezService, CezService>();
        services.AddScoped<IQuizGenerationService, QuizGenerationService>();
        services.AddScoped<IFlashcardGenerationService, FlashcardGenerationService>();
        services.AddScoped<IJobService, JobService>();
        return services;
    }
}
