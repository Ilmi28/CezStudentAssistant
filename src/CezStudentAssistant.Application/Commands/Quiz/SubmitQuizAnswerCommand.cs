using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class SubmitQuizAnswerCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizAttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}

public class SubmitQuizAnswerCommandHandler : BaseCommandHandler<SubmitQuizAnswerCommand>
{
    protected override string SuccessMessage => QuizMessageConsts.AnswerSubmittedSuccess;

    protected override string ErrorMessage => QuizMessageConsts.AnswerSubmittedError;

    protected override Task ExecuteAsync(SubmitQuizAnswerCommand command, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
