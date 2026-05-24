namespace CezStudentAssistant.Application.Interfaces.CQRS;

/// <summary>
/// Represents a query operation that can be executed or processed by a handler.
/// </summary>
/// <remarks>Implement this interface to define a query type for use with a query handling or mediator
/// pattern. Queries typically represent read-only requests that return data without modifying application
/// state.</remarks>
public interface IQuery { }
