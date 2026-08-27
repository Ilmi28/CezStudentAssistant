using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Quiz;

public class UpdateQuizCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public required string Name { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? QuestionCountPerAttempt { get; set; }
    public int? EasyQuestionCountPerAttempt { get; set; }
    public int? MediumQuestionCountPerAttempt { get; set; }
    public int? HardQuestionCountPerAttempt { get; set; }
}

public class UpdateQuizCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<UpdateQuizCommand>
{
    protected override string SuccessMessage => QuizMessageConsts.UpdateQuizSuccess;
    protected override string ErrorMessage => QuizMessageConsts.UpdateQuizError;

    protected override async Task ExecuteAsync(UpdateQuizCommand command, CancellationToken ct)
    {
        var quizRepository = unitOfWork.Repository<IQuizRepository>();
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, ct);

        if (quiz == null)
        {
            throw new NotFoundException(QuizMessageConsts.QuizNotFound);
        }

        if (quiz.UserId != command.UserId)
        {
            throw new UnauthorizedException(QuizMessageConsts.QuizAccessDenied);
        }

        quiz.Name = command.Name;
        quiz.TimeLimitMinutes = command.TimeLimitMinutes;
        quiz.QuestionCountPerAttempt = command.QuestionCountPerAttempt;
        quiz.EasyQuestionCountPerAttempt = command.EasyQuestionCountPerAttempt;
        quiz.MediumQuestionCountPerAttempt = command.MediumQuestionCountPerAttempt;
        quiz.HardQuestionCountPerAttempt = command.HardQuestionCountPerAttempt;

        await quizRepository.UpdateAsync(quiz, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
