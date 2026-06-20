using CezStudentAssistant.Application.Interfaces.Services;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.IntegrationTests;

internal sealed class ScopedImmediateJobScheduler(IServiceScopeFactory scopeFactory) : IJobScheduler
{
    public void Enqueue(Expression<Action> methodCall) => Invoke(methodCall, null);

    public void Enqueue<T>(Expression<Action<T>> methodCall) => InvokeInScope(methodCall);

    private void InvokeInScope<T>(Expression<Action<T>> methodCall)
    {
        using var scope = scopeFactory.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService(typeof(T));
        Invoke(methodCall, target);
    }

    private static void Invoke(LambdaExpression methodCall, object? target)
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
    }
}
