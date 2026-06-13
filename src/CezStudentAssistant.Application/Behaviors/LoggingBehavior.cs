using MediatR;
using Microsoft.Extensions.Logging;

namespace CezStudentAssistant.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const string Separator = "==================================================";

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation(
            "\n{Separator}\n[MEDIATR] START: Handling {RequestName}\nPayload: {@Payload}\n{Separator}\n",
            Separator,
            requestName,
            request,
            Separator
        );

        try
        {
            var response = await next();

            logger.LogInformation(
                "\n{Separator}\n[MEDIATR] END: Successfully handled {RequestName}\n{Separator}\n",
                Separator,
                requestName,
                Separator
            );

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "\n{Separator}\n[MEDIATR] ERROR: Failed to handle {RequestName}\nMessage: {ErrorMessage}\n{Separator}\n",
                Separator,
                requestName,
                ex.Message,
                Separator
            );

            throw;
        }
    }
}
