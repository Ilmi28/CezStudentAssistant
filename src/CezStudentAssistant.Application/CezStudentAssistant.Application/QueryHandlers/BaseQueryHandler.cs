using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.QueryHandlers;

public abstract class BaseQueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery
{
    protected abstract ApiMessage SuccessMessage { get; }

    protected abstract ApiMessage ErrorMessage { get; }

    protected abstract Task<TResponse> ExecuteAsync(TQuery query, CancellationToken ct);

    public async Task<ApiResponse<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            TResponse result = await ExecuteAsync(query, cancellationToken);

            return new SuccessResponse<TResponse>(SuccessMessage, result);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException(ErrorMessage, ex);
        }
    }
}
