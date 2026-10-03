using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Quiz;

public class DeleteQuizCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
}

public class DeleteQuizCommandHandler(IUnitOfWork unitOfWork, ICascadeDeleteService cascadeDeleteService) : BaseCommandHandler<DeleteQuizCommand>
{
    protected override string SuccessMessage => QuizMessageConsts.DeleteQuizSuccess;
    protected override string ErrorMessage => QuizMessageConsts.DeleteQuizError;

    protected override async Task ExecuteAsync(DeleteQuizCommand command, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var quiz = await quizRepo.GetByIdAsync(command.QuizId, ct);

        if (quiz == null)
        {
            throw new NotFoundException(QuizMessageConsts.QuizNotFound);
        }

        if (quiz.UserId != command.UserId)
        {
            throw new UnauthorizedException(QuizMessageConsts.QuizAccessDenied);
        }

        await cascadeDeleteService.DeleteQuizCascadeAsync(quiz, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
