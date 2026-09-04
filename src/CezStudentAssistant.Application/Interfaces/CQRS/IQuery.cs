using CezStudentAssistant.Application.Responses;
using MediatR;

namespace CezStudentAssistant.Application.Interfaces.CQRS;

public interface IQuery<TResponse> : IRequest<ApiResponse<TResponse>> { }

