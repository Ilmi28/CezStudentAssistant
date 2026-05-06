using CezStudentAssistant.Application.Interfaces.External;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Cez;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCez()
        {
            services.AddHttpClient<ICezRequestExecutor, CezRequestExecutor>(client =>
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CEZ_API_BASE_URL") ?? string.Empty);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<ICezApiClient, CezApiClient>();

            return services;
        }
    }
}
