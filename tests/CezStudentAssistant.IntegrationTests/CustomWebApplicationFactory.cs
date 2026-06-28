using CezStudentAssistant.API;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using Azure.Storage.Blobs;

namespace CezStudentAssistant.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly PostgreSqlContainer DatabaseContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("cezstudentassistant_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private static readonly IContainer BlobContainer = new ContainerBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.33.0")
        .WithPortBinding(10000, true)
        .WithCommand("azurite-blob", "--blobHost", "0.0.0.0", "--skipApiVersionCheck")
        .Build();

    static CustomWebApplicationFactory()
    {
        DatabaseContainer.StartAsync().GetAwaiter().GetResult();
        BlobContainer.StartAsync().GetAwaiter().GetResult();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DatabaseContainer.GetConnectionString());

        var blobConnectionString = $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://{BlobContainer.Hostname}:{BlobContainer.GetMappedPublicPort(10000)}/devstoreaccount1;";
        Environment.SetEnvironmentVariable("ConnectionStrings__AzureBlobStorage", blobConnectionString);

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

        CezApiClientMock.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse
            {
                Success = true,
                Data = DefaultCezCourses.ToList()
            });

        CezApiClientMock.GetCourseContent(Arg.Any<CezCourseRequest>())
            .Returns(new CezCourseContentResponse
            {
                Success = true,
                Data = new List<CezCourseContent>()
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

        var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
        var containerClient = blobServiceClient.GetBlobContainerClient("course-files");
        containerClient.CreateIfNotExists();
    }
}
