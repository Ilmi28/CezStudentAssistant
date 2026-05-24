using CezStudentAssistant.API;
using CezStudentAssistant.Application.Interfaces.External;
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "");

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
