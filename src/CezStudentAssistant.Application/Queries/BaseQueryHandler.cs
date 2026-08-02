using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;
using MediatR;

namespace CezStudentAssistant.Application.Queries;

public abstract class BaseQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, ApiResponse<TResponse>>
    where TQuery : IQuery<TResponse>
{
    protected abstract string SuccessMessage { get; }

    protected abstract string ErrorMessage { get; }

    protected abstract Task<TResponse> ExecuteAsync(TQuery query, CancellationToken ct);

    public async Task<ApiResponse<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
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
