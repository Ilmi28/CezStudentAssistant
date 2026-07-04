using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.Services;
using Hangfire;
using System.Linq.Expressions;

namespace CezStudentAssistant.Infrastructure.Services;

public class HangfireJobScheduler(IBackgroundJobClient backgroundJobClient) : IJobScheduler, IScopedService
{
    public string Enqueue(Expression<Action> methodCall) =>
        backgroundJobClient.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Action<T>> methodCall) =>
        backgroundJobClient.Enqueue<T>(methodCall);
}
