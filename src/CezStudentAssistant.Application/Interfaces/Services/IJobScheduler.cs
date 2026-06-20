using System.Linq.Expressions;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IJobScheduler
{
    void Enqueue(Expression<Action> methodCall);
    void Enqueue<T>(Expression<Action<T>> methodCall);
}
