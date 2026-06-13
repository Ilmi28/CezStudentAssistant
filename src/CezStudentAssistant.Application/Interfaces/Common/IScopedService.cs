namespace CezStudentAssistant.Application.Interfaces.Common;

/// <summary>
/// Defines a contract for services that are intended to be instantiated once per scope, such as per web request or per
/// operation.
/// </summary>
/// <remarks>Implement this interface to indicate that a service should have scoped lifetime when registered with
/// a dependency injection container. Scoped services are created once per scope and disposed when the scope ends. This
/// is commonly used in web applications to ensure services are unique per HTTP request.</remarks>
public interface IScopedService { }
