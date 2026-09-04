using CezStudentAssistant.Application.Responses;
using MediatR;

namespace CezStudentAssistant.Application.Interfaces.CQRS;

public interface ICommand : IRequest<ApiResponse> { }

public interface ICommand<TResponse> : IRequest<ApiResponse<TResponse>> { }

