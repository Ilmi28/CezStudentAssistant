using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using MediatR;

namespace CezStudentAssistant.Application.Behaviors;

public class UserContextBehavior<TRequest, TResponse>(ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUserRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.GetCurrentUserId();

        if (currentUserId.HasValue)
        {
            request.UserId = currentUserId.Value;
        }

        return await next();
    }
}
