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
            services.AddSingleton(x => new Client(apiKey: configuration["Gemini:ApiKey"]));

            return services;
        }
    }
}
