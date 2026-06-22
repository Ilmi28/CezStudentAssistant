using CezStudentAssistant.API;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace CezStudentAssistant.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly PostgreSqlContainer DatabaseContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("cezstudentassistant_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    static CustomWebApplicationFactory()
    {
        DatabaseContainer.StartAsync().GetAwaiter().GetResult();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DatabaseContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("Cez__ApiBaseUrl", "https://cez.test/");
    }

    public ICezApiClient CezApiClientMock { get; } = Substitute.For<ICezApiClient>();
    public static IReadOnlyList<CezCourse> DefaultCezCourses { get; } = new List<CezCourse>
    {
        new() { ExternalId = 101, DisplayName = "Calculus I" },
        new() { ExternalId = 102, DisplayName = "Physics II" }
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        CezApiClientMock.GetUserCourses(Arg.Any<CezStudentAssistant.Application.Requests.Cez.CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse
            {
                Success = true,
                Data = DefaultCezCourses.ToList()
            });

        builder.ConfigureServices(services =>
        {
            // Mock External CEZ API
            services.AddSingleton(CezApiClientMock);
            services.AddScoped<IJobScheduler, ScopedImmediateJobScheduler>();
        });
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.Migrate();
    }
}
