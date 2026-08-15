using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.AI.Services;
using Google.GenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.AI;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAI(IConfiguration configuration)
        {
            var apiKey = configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Configuration 'Gemini:ApiKey' is missing or empty.");
            services.AddSingleton(x => new Client(apiKey: apiKey));
            services.AddScoped<IFileContentProcessorService, FileContentProcessorService>();
            services.AddScoped<IAIQuizService, AIQuizService>();
            services.AddScoped<IAIClient, GeminiAIClient>();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(AIProfile).Assembly);
            });

            return services;
        }
    }
}
