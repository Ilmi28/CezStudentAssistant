using CezStudentAssistant.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace CezStudentAssistant.IntegrationTests;

internal sealed class ScopedImmediateJobSchedulerService(IServiceScopeFactory scopeFactory) : IJobSchedulerService
{
    public string Enqueue(Expression<Action> methodCall) => Invoke(methodCall, null);

    public string Enqueue<T>(Expression<Action<T>> methodCall) => InvokeInScope(methodCall);

    private string InvokeInScope<T>(Expression<Action<T>> methodCall)
    {
        using var scope = scopeFactory.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService(typeof(T));
        Invoke(methodCall, target);
        return Guid.NewGuid().ToString();
    }

    private static string Invoke(LambdaExpression methodCall, object? target)
    {
        if (methodCall.Body is not MethodCallExpression call)
        {
            throw new InvalidOperationException("Unsupported job expression.");
        }

        var arguments = call.Arguments
            .Select(argument => Expression.Lambda(argument).Compile().DynamicInvoke())
            .ToArray();

        var parameterTypes = call.Method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        var method = target?.GetType().GetMethod(call.Method.Name, parameterTypes) ?? call.Method;
        var result = method.Invoke(target, arguments);

        if (result is Task task)
        {
            task.GetAwaiter().GetResult();
        }

        return Guid.NewGuid().ToString();
    }
}
