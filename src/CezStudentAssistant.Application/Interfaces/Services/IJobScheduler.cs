using System.Linq.Expressions;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IJobScheduler
{
    string Enqueue(Expression<Action> methodCall);
    string Enqueue<T>(Expression<Action<T>> methodCall);
    string ContinueWith(string parentJobId, Expression<Action> methodCall);
    string ContinueWith<T>(string parentJobId, Expression<Action<T>> methodCall);
}
