using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Cez.Interfaces;
using CezStudentAssistant.Cez.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Cez;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCez()
        {
            services.AddHttpClient<ICezRequestService, CezRequestService>(client =>
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CEZ_API_BASE_URL") ?? string.Empty);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<ICezApiClient, CezApiClient>();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(CezProfile).Assembly);
            });

            return services;
        }
    }
}
