
using CezStudentAssistant.API.Commands;
using CezStudentAssistant.API.Extensions;
using CezStudentAssistant.API.Interfaces.CQRS;
using CezStudentAssistant.API.Queries;
using CezStudentAssistant.API.QueryHandlers;
using CezStudentAssistant.API.Responses;
using FluentValidation;

namespace CezStudentAssistant.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services);

            var app = builder.Build();

            MapEndpoints(app);
            ConfigureMiddleware(app);

            app.Run();
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.AddCQRSHandlers();
            services.AddLoggingDecorator();
            services.AddValidatorsFromAssemblyContaining<Program>();
        }

        private static void MapEndpoints(WebApplication app)
        {
            app.MapGet("/example-query", async ([AsParameters] ExampleQuery query, IQueryHandler<ExampleQuery, ExampleResponse> handler) 
                => await handler.HandleAsync(query));

            app.MapPost("/example-command", async (ExampleCommand query, ICommandHandler<ExampleCommand, ExampleResponse> handler)
                => await handler.HandleAsync(query));

            app.MapPost("/example-command-without-response", async (ExampleCommand query, ICommandHandler<ExampleCommand> handler)
                => await handler.HandleAsync(query));

        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
        }
    }
}
