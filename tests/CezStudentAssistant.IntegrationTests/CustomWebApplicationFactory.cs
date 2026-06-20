using CezStudentAssistant.API;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Data.Common;

namespace CezStudentAssistant.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public ICezApiClient CezApiClientMock { get; } = Substitute.For<ICezApiClient>();
    public static IReadOnlyList<CezCourse> DefaultCezCourses { get; } = new List<CezCourse>
    {
        new() { ExternalId = 101, DisplayName = "Calculus I" },
        new() { ExternalId = 102, DisplayName = "Physics II" }
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "");
        Environment.SetEnvironmentVariable("CEZ_API_BASE_URL", "https://cez.test/");

        CezApiClientMock.GetUserCourses(Arg.Any<CezStudentAssistant.Application.Requests.Cez.CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse
            {
                Success = true,
                Data = DefaultCezCourses.ToList()
            });

        builder.ConfigureServices(services =>
        {
            // Create open SQLite connection so it isn't closed between calls
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<AppDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

            // Mock External CEZ API
            services.AddSingleton(CezApiClientMock);
            services.AddScoped<IJobScheduler, ScopedImmediateJobScheduler>();
        });

        builder.UseEnvironment("Development");
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}
