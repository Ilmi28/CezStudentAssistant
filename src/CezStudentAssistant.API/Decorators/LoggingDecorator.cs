using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.API.Decorators;

public static class LoggingDecorator
{
    private const string Separator = "==================================================";

    public sealed class CommandHandlerDecorator<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandlerDecorator<TCommand, TResponse>> logger
    ) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand
    {
        public async Task<ApiResponse<TResponse>> HandleAsync(
            TCommand command,
            CancellationToken cancellationToken = default
        )
        {
            var typeName = typeof(TCommand).Name;

            logger.LogInformation(
                "\n{Separator}\n[COMMAND] START: Handling query {CommandType}\nPayload: {@CommandPayload}\n{Separator}\n",
                Separator,
                typeName,
                command,
                Separator
            );

            try
            {
                var response = await innerHandler.HandleAsync(command, cancellationToken);

                logger.LogInformation(
                    "\n{Separator}\n[COMMAND] END: Successfully handled {CommandType}\n{Separator}\n",
                    Separator,
                    typeName,
                    Separator
                );

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "\n{Separator}\n[COMMAND] ERROR: Failed to handle {CommandType}\nMessage: {ErrorMessage}\n{Separator}\n",
                    Separator,
                    typeName,
                    ex.Message,
                    Separator
                );

                throw;
            }
        }
    }

    public sealed class CommandHandlerDecorator<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandHandlerDecorator<TCommand>> logger
    ) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<ApiResponse> HandleAsync(
            TCommand command,
            CancellationToken cancellationToken = default
        )
        {
            var typeName = typeof(TCommand).Name;

            logger.LogInformation(
                "\n{Separator}\n[COMMAND] START: Handling command {CommandType}\nPayload: {@CommandPayload}\n{Separator}\n",
                Separator,
                typeName,
                command,
                Separator
            );

            try
            {
                var response = await innerHandler.HandleAsync(command, cancellationToken);

                logger.LogInformation(
                    "\n{Separator}\n[COMMAND] END: Successfully handled {CommandType}\n{Separator}\n",
                    Separator,
                    typeName,
                    Separator
                );

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "\n{Separator}\n[COMMAND] ERROR: Failed to handle {CommandType}\nMessage: {ErrorMessage}\n{Separator}\n",
                    Separator,
                    typeName,
                    ex.Message,
                    Separator
                );

                throw;
            }
        }
    }

    public sealed class QueryHandlerDecorator<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandlerDecorator<TQuery, TResponse>> logger
    ) : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery
        where TResponse : class
    {
        public async Task<ApiResponse<TResponse>> HandleAsync(
            TQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var typeName = typeof(TQuery).Name;

            logger.LogInformation(
                "\n{Separator}\n[QUERY] START: Handling query {QueryType}\n{Separator}\n",
                Separator,
                typeName,
                Separator
            );

            try
            {
                var response = await innerHandler.HandleAsync(query, cancellationToken);

                logger.LogInformation(
                    "\n{Separator}\n[QUERY] END: Successfully handled {QueryType}\n{Separator}\n",
                    Separator,
                    typeName,
                    Separator
                );

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "\n{Separator}\n[QUERY] ERROR: Failed to handle {QueryType}\nMessage: {ErrorMessage}\n{Separator}\n",
                    Separator,
                    typeName,
                    ex.Message,
                    Separator
                );

                throw;
            }
        }
    }
}
