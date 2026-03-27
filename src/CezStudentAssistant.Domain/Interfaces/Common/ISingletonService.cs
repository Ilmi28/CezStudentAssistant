namespace CezStudentAssistant.Domain.Interfaces.Common;

/// <summary>
/// Defines a contract for services that are intended to be registered and used as singletons within the application's
/// dependency injection container.
/// </summary>
/// <remarks>Implementing this interface indicates that a service should have a single, shared instance throughout
/// the application's lifetime. This is typically used to ensure consistent state or resource management across the
/// application.</remarks>
public interface ISingletonService
{
}
