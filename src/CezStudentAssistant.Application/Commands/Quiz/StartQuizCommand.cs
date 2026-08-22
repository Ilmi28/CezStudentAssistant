using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class StartQuizCommand : ICommand<QuizAttemptDetailsDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
}

public class StartQuizCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<StartQuizCommand, QuizAttemptDetailsDto>
{
    protected override string SuccessMessage => QuizMessageConsts.StartQuizSuccess;

    protected override string ErrorMessage => QuizMessageConsts.StartQuizError;

    protected override async Task<QuizAttemptDetailsDto> ExecuteAsync(StartQuizCommand command, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var quiz = await quizRepo.Find(q => q.Id == command.QuizId && (q.UserId == command.UserId || q.Course.Users.Any(u => u.Id == command.UserId)))
            .Include(q => q.Course)
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(ct);

        if (quiz == null)
            throw new NotFoundException(QuizMessageConsts.QuizNotFound);

        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var attempt = new QuizAttempt
        {
            QuizId = quiz.Id,
            Course = quiz.Course,
            UserId = command.UserId,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };

        await quizAttemptRepo.AddAsync(attempt, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new QuizAttemptDetailsDto
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            DisplayName = quiz.DisplayName,
            CourseName = quiz.Course.Name,
            Status = attempt.Status,
            IsPending = true,
            StartedAt = attempt.StartedAt,
            ExpiresAt = attempt.ExpiresAt,
            Questions = quiz.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Content = q.Content,
                Type = q.Type,
                Difficulty = q.Difficulty,
                Points = q.Points,
                Options = q.Options.Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList(),
            Answers = new List<QuestionAnswerDto>()
        };
    }
}
