using System.Linq.Expressions;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IJobSchedulerService
{
    string Enqueue(Expression<Action> methodCall);
    string Enqueue<T>(Expression<Action<T>> methodCall);
}
