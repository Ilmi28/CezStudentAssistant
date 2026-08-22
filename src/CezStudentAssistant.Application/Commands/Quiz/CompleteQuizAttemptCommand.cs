using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class CompleteQuizAttemptCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizAttemptId { get; set; }
}

public class CompleteQuizAttemptCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<CompleteQuizAttemptCommand>
{
    protected override string SuccessMessage => QuizMessageConsts.CompleteQuizAttemptSuccess;

    protected override string ErrorMessage => QuizMessageConsts.CompleteQuizAttemptError;

    protected override async Task ExecuteAsync(CompleteQuizAttemptCommand command, CancellationToken ct)
    {
        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();

        var quizAttempt = await quizAttemptRepo.Find(a => a.Id == command.QuizAttemptId && a.UserId == command.UserId)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOptions)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(ct);

        if (quizAttempt == null)
            throw new NotFoundException(QuizMessageConsts.QuizAttemptNotFound);

        if (quizAttempt.Status != QuizAttemptStatus.InProgress)
            throw new BadRequestException(QuizMessageConsts.QuizAttemptNotInProgress);

        quizAttempt.Points = QuizPointsCalculationHelper.CalculatePoints(quizAttempt);
        quizAttempt.Status = QuizAttemptStatus.Completed;

        await unitOfWork.SaveChangesAsync(ct);
    }
}
