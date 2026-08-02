using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace CezStudentAssistant.ArchitectureTests;

[TestFixture]
public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Entities.BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(API.Interfaces.ICezSyncNotificationClient).Assembly;
    private static readonly Assembly AiAssembly = typeof(AI.DependencyInjection).Assembly;
    private static readonly Assembly CezAssembly = typeof(Cez.DependencyInjection).Assembly;

    [Test]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        var otherProjects = new[]
        {
            "CezStudentAssistant.Application",
            "CezStudentAssistant.Infrastructure",
            "CezStudentAssistant.API",
            "CezStudentAssistant.AI",
            "CezStudentAssistant.Cez"
        };

        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing Domain type (violates isolation): {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue("Domain layer must not depend on any other layers or external services.");
    }

    [Test]
    public void Application_ShouldNotHaveDependencyOnOtherProjects()
    {
        var otherProjects = new[]
        {
            "CezStudentAssistant.Infrastructure",
            "CezStudentAssistant.API",
            "CezStudentAssistant.AI",
            "CezStudentAssistant.Cez"
        };

        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing Application type (violates isolation): {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue("Application layer must not depend on Infrastructure, API, or External Services.");
    }

    [Test]
    public void Infrastructure_ShouldNotHaveDependencyOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("CezStudentAssistant.API")
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing Infrastructure type (violates isolation): {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue("Infrastructure layer must not depend on the API layer.");
    }

    [Test]
    public void ExternalServices_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        var otherProjects = new[]
        {
            "CezStudentAssistant.Infrastructure",
            "CezStudentAssistant.API"
        };

        var aiResult = Types.InAssembly(AiAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        if (!aiResult.IsSuccessful && aiResult.FailingTypes != null)
        {
            foreach (var type in aiResult.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing AI Service type (violates isolation): {type.FullName}");
            }
        }

        var cezResult = Types.InAssembly(CezAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        if (!cezResult.IsSuccessful && cezResult.FailingTypes != null)
        {
            foreach (var type in cezResult.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing Cez Service type (violates isolation): {type.FullName}");
            }
        }

        aiResult.IsSuccessful.Should().BeTrue("AI External Service must not depend on Infrastructure or API.");
        cezResult.IsSuccessful.Should().BeTrue("Cez External Service must not depend on Infrastructure or API.");
    }

    [Test]
    public void Entities_ShouldInheritFromBaseEntity()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("CezStudentAssistant.Domain.Entities")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("BaseEntity")
            .Should()
            .Inherit(typeof(Domain.Entities.BaseEntity))
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing entity type (does not inherit from BaseEntity): {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue("All entities in CezStudentAssistant.Domain.Entities namespace must inherit from BaseEntity.");
    }

    [Test]
    public void Repositories_ShouldBeInterfacesInProperNamespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreInterfaces()
            .Should()
            .ResideInNamespace("CezStudentAssistant.Domain.Interfaces.Repositories")
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing repository interface (not in proper namespace): {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue("All repository interfaces must reside in CezStudentAssistant.Domain.Interfaces.Repositories namespace.");
    }

    [Test]
    public void Handlers_ShouldHaveNamesEndingWithHandler()
    {
        var handlerTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i =>
                i.IsGenericType && (
                    i.GetGenericTypeDefinition().Name.StartsWith("IRequestHandler") ||
                    i.GetGenericTypeDefinition().Name.StartsWith("INotificationHandler")
                )))
            .ToList();

        var failingTypes = handlerTypes
            .Where(t => !t.Name.EndsWith("Handler"))
            .Select(t => t.FullName)
            .ToList();

        if (failingTypes.Any())
        {
            foreach (var name in failingTypes)
            {
                TestContext.Out.WriteLine($"Failing handler name (missing 'Handler' suffix): {name}");
            }
        }

        failingTypes.Should().BeEmpty("all MediatR request or notification handlers in Application should have names ending with 'Handler'.");
    }

    [Test]
    public void Commands_ShouldHaveNameEndingWithCommand()
    {
        var commandTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass || (t.IsValueType && !t.IsEnum))
            .Where(t => t.GetInterfaces().Any(i =>
                i.Name == "ICommand" ||
                (i.IsGenericType && i.GetGenericTypeDefinition().Name == "ICommand`1")))
            .ToList();

        var failingTypes = commandTypes
            .Where(t => !t.Name.EndsWith("Command"))
            .Select(t => t.FullName)
            .ToList();

        if (failingTypes.Any())
        {
            foreach (var name in failingTypes)
            {
                TestContext.Out.WriteLine($"Failing command name (missing 'Command' suffix): {name}");
            }
        }

        failingTypes.Should().BeEmpty("all command types in Application should have names ending with 'Command'.");
    }

    [Test]
    public void Queries_ShouldHaveNameEndingWithQuery()
    {
        var queryTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass || (t.IsValueType && !t.IsEnum))
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition().Name == "IQuery`1"))
            .ToList();

        var failingTypes = queryTypes
            .Where(t => !t.Name.EndsWith("Query"))
            .Select(t => t.FullName)
            .ToList();

        if (failingTypes.Any())
        {
            foreach (var name in failingTypes)
            {
                TestContext.Out.WriteLine($"Failing query name (missing 'Query' suffix): {name}");
            }
        }

        failingTypes.Should().BeEmpty("all query types in Application should have names ending with 'Query'.");
    }

    [Test]
    public void ApiEndpoints_ShouldNotDependOnRepositories()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("CezStudentAssistant.API.Endpoints")
            .ShouldNot()
            .HaveDependencyOn("CezStudentAssistant.Domain.Interfaces.Repositories")
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing API Endpoint type (depends on repository): {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue("API endpoint classes should not reference repositories directly; they should use IMediator/ISender.");
    }

    [Test]
    public void Interfaces_ShouldStartWithI()
    {
        var result = Types.InAssemblies(new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly, AiAssembly, CezAssembly })
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        if (!result.IsSuccessful && result.FailingTypes != null)
        {
            foreach (var type in result.FailingTypes)
            {
                TestContext.Out.WriteLine($"Failing interface name (does not start with 'I'): {type.FullName}");
            }
        }

        result.IsSuccessful.Should().BeTrue("All interface names in the codebase must start with 'I'.");
    }

    [Test]
    public void Validators_ShouldHaveNamesEndingWithValidator()
    {
        var validatorTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(FluentValidation.IValidator).IsAssignableFrom(t))
            .ToList();

        var failingTypes = validatorTypes
            .Where(t => !t.Name.EndsWith("Validator"))
            .Select(t => t.FullName)
            .ToList();

        if (failingTypes.Any())
        {
            foreach (var name in failingTypes)
            {
                TestContext.Out.WriteLine($"Failing validator name (missing 'Validator' suffix): {name}");
            }
        }

        failingTypes.Should().BeEmpty("all validation classes in Application should have names ending with 'Validator'.");
    }

    [Test]
    public void Notifications_ShouldHaveNameEndingWithNotification()
    {
        var notificationTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass || (t.IsValueType && !t.IsEnum))
            .Where(t => t.GetInterfaces().Any(i => i == typeof(MediatR.INotification)))
            .ToList();

        var failingTypes = notificationTypes
            .Where(t => !t.Name.EndsWith("Notification"))
            .Select(t => t.FullName)
            .ToList();

        if (failingTypes.Any())
        {
            foreach (var name in failingTypes)
            {
                TestContext.Out.WriteLine($"Failing notification name (missing 'Notification' suffix): {name}");
            }
        }

        failingTypes.Should().BeEmpty("all MediatR notifications in Application should have names ending with 'Notification'.");
    }

    [Test]
    public void ConcreteRepositories_ShouldEndWithRepositoryAndBeInPersistenceRepositoriesNamespace()
    {
        var repositoryInterfaceType = typeof(CezStudentAssistant.Domain.Interfaces.Repositories.IGenericRepository<>);
        var repositoryInterfaces = DomainAssembly.GetTypes()
            .Where(t => t.IsInterface && (t.Namespace == "CezStudentAssistant.Domain.Interfaces.Repositories" || t.Name.EndsWith("Repository")))
            .ToList();

        var concreteRepos = InfrastructureAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && t.GetInterfaces().Any(i =>
                repositoryInterfaces.Contains(i) ||
                (i.IsGenericType && i.GetGenericTypeDefinition() == repositoryInterfaceType)))
            .ToList();

        var failingTypes = concreteRepos
            .Where(t => !t.Name.EndsWith("Repository") || t.Namespace != "CezStudentAssistant.Infrastructure.Persistence.Repositories")
            .Select(t => t.FullName)
            .ToList();

        if (failingTypes.Any())
        {
            foreach (var name in failingTypes)
            {
                TestContext.Out.WriteLine($"Failing concrete repository (name or namespace mismatch): {name}");
            }
        }

        failingTypes.Should().BeEmpty("all concrete repositories in Infrastructure must reside in CezStudentAssistant.Infrastructure.Persistence.Repositories namespace and end with 'Repository'.");
    }

    [Test]
    public void ConcreteServicesAndSchedulers_ShouldHaveProperNaming()
    {
        var allAssemblies = new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly, AiAssembly, CezAssembly };
        var concreteTypes = allAssemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsClass && !t.IsAbstract).ToList();

        var markerInterfaceNames = new[] { "IScopedService", "ISingletonService", "ITransientService" };

        var failingServices = concreteTypes
            .Where(t => t.GetInterfaces().Any(i =>
                i.Namespace != null &&
                i.Namespace.StartsWith("CezStudentAssistant") &&
                !markerInterfaceNames.Contains(i.Name) &&
                i.Name.EndsWith("Service"))
                && !t.Name.EndsWith("Service"))
            .Select(t => t.FullName)
            .ToList();

        var failingSchedulers = concreteTypes
            .Where(t => t.GetInterfaces().Any(i =>
                i.Namespace != null &&
                i.Namespace.StartsWith("CezStudentAssistant") &&
                !markerInterfaceNames.Contains(i.Name) &&
                i.Name.EndsWith("Scheduler"))
                && !t.Name.EndsWith("Scheduler"))
            .Select(t => t.FullName)
            .ToList();

        if (failingServices.Any())
        {
            foreach (var name in failingServices)
            {
                TestContext.Out.WriteLine($"Failing service implementation (does not end with 'Service'): {name}");
            }
        }

        if (failingSchedulers.Any())
        {
            foreach (var name in failingSchedulers)
            {
                TestContext.Out.WriteLine($"Failing scheduler implementation (does not end with 'Scheduler'): {name}");
            }
        }

        failingServices.Should().BeEmpty("all classes implementing a custom Service interface must end with 'Service'.");
        failingSchedulers.Should().BeEmpty("all classes implementing a custom Scheduler interface must end with 'Scheduler'.");
    }

    [Test]
    public void CustomExceptions_ShouldNotUseStringLiteralsDirectly()
    {
        // Arrange
        var solutionRoot = FindSolutionRoot();
        var srcDir = Path.Combine(solutionRoot, "src");

        var csFiles = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories);
        var failingFiles = new List<string>();

        // Regex to match instantiation of our custom exceptions with a string literal ("...", $"...", @"...", etc.)
        var literalRegex = new System.Text.RegularExpressions.Regex(
            @"\bnew\s+(NotFound|Conflict|BadRequest|Forbidden|Unauthorized|BadGateway|App|ApiValidation)Exception\s*\(\s*[\$@]*""",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        // Act
        foreach (var file in csFiles)
        {
            if (file.Contains(@"\obj\") || file.Contains(@"\bin\"))
            {
                continue;
            }

            var content = File.ReadAllText(file);
            if (literalRegex.IsMatch(content))
            {
                failingFiles.Add(Path.GetFileName(file));
            }
        }

        // Assert
        failingFiles.Should().BeEmpty(
            "Custom exceptions (AppException, NotFoundException, etc.) must not use hardcoded string literals directly. " +
            "Instead, define them in a constant class (e.g. CourseMessageConsts or GeneralMessageConsts) and reference the constant.");
    }

    [Test]
    public void Handlers_ShouldNotUseStringLiteralsForSuccessAndErrorMessages()
    {
        // Arrange
        var solutionRoot = FindSolutionRoot();
        var srcDir = Path.Combine(solutionRoot, "src");

        var csFiles = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories);
        var failingFiles = new List<string>();

        // Regex to match property overrides of SuccessMessage or ErrorMessage returning a string literal directly
        var literalRegex = new System.Text.RegularExpressions.Regex(
            @"\bprotected\s+override\s+string\s+(Success|Error)Message\s*=>\s*[\$@]*""",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        // Act
        foreach (var file in csFiles)
        {
            if (file.Contains(@"\obj\") || file.Contains(@"\bin\"))
            {
                continue;
            }

            var content = File.ReadAllText(file);
            if (literalRegex.IsMatch(content))
            {
                failingFiles.Add(Path.GetFileName(file));
            }
        }

        // Assert
        failingFiles.Should().BeEmpty(
            "Command and Query Handlers must not use hardcoded string literals directly for SuccessMessage or ErrorMessage. " +
            "Instead, reference a constant (e.g. QuizMessageConsts.AnswerSubmittedSuccess).");
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(typeof(ArchitectureTests).Assembly.Location)!);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CezStudentAssistant.slnx")) && !File.Exists(Path.Combine(dir.FullName, "CezStudentAssistant.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new DirectoryNotFoundException("Solution root could not be found.");
    }
}
