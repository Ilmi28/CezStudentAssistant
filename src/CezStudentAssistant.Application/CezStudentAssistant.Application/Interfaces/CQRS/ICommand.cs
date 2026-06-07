using CezStudentAssistant.Application.Responses;
using MediatR;

namespace CezStudentAssistant.Application.Interfaces.CQRS;

/// <summary>
/// Defines a command in the CQRS pattern.
/// </summary>
public interface ICommand : IRequest<ApiResponse> { }

/// <summary>
/// Defines a command with a response in the CQRS pattern.
/// </summary>
public interface ICommand<TResponse> : IRequest<ApiResponse<TResponse>> { }
