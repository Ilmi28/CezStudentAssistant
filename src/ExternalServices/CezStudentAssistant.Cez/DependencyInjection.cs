using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Cez.Interfaces;
using CezStudentAssistant.Cez.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Cez;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCez(IConfiguration configuration)
        {
            services.AddHttpClient<ICezRequestService, CezRequestService>(client =>
            {
                client.BaseAddress = new Uri(configuration["Cez:ApiBaseUrl"] ?? string.Empty);
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
