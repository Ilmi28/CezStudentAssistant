using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class StartQuizCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizAttemptId { get; set; }
}

public class StartQuizCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<StartQuizCommand>
{
    protected override string SuccessMessage => QuizMessageConsts.StartQuizSuccess;

    protected override string ErrorMessage => QuizMessageConsts.StartQuizError;

    protected override async Task ExecuteAsync(StartQuizCommand command, CancellationToken ct)
    {
        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var quizAttempt = await quizAttemptRepo.GetByIdAsync(command.QuizAttemptId, ct)
            ?? throw new NotFoundException(QuizMessageConsts.QuizAttemptNotFound);

        if (command.UserId != quizAttempt.UserId)
            throw new ForbiddenException(QuizMessageConsts.UnauthorizedStartAccess);

        if (quizAttempt.Status != Domain.Enums.QuizAttemptStatus.Ready)
            throw new BadRequestException(QuizMessageConsts.QuizAttemptNotReady);

        quizAttempt.Status = Domain.Enums.QuizAttemptStatus.InProgress;
        quizAttempt.StartedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(ct);
    }
}
