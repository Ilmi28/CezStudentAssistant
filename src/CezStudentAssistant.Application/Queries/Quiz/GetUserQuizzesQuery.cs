using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Quiz;

public sealed class GetUserQuizzesQuery : IQuery<List<QuizDto>>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetUserQuizzesQueryHandler(IUnitOfWork unitOfWork) : BaseQueryHandler<GetUserQuizzesQuery, List<QuizDto>>
{
    protected override string SuccessMessage => QuizMessageConsts.GetQuizzesSuccess;

    protected override string ErrorMessage => QuizMessageConsts.GetQuizzesError;

    protected override async Task<List<QuizDto>> ExecuteAsync(GetUserQuizzesQuery query, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();

        var quizzes = await quizRepo.Find(q => q.UserId == query.UserId)
            .Include(q => q.Course)
            .Include(q => q.Questions)
                .ThenInclude(qn => qn.Options)
            .Include(q => q.Attempts)
                .ThenInclude(a => a.Answers)
                    .ThenInclude(ans => ans.SelectedOptions)
            .ToListAsync(ct);

        return quizzes.Select(q =>
        {
            var userAttempts = q.Attempts.Where(a => a.UserId == query.UserId).ToList();
            var lastAttempt = userAttempts.OrderByDescending(a => a.StartedAt).FirstOrDefault();
            var mastery = QuizMasteryCalculationHelper.CalculateMastery(q.Questions, userAttempts);

            return new QuizDto
            {
                Id = q.Id,
                UserId = q.UserId,
                Name = q.Name,
                CourseId = q.CourseId,
                CourseName = q.Course?.Name ?? string.Empty,
                Status = q.Status,
                TimeLimitMinutes = q.TimeLimitMinutes,
                MaxPoints = q.Questions.Sum(qn => qn.Difficulty == QuestionDifficulty.Easy ? 1m : qn.Difficulty == QuestionDifficulty.Hard ? 3m : 2m),
                LastAttemptStatus = lastAttempt?.Status,
                LastAttemptExpiresAt = lastAttempt?.ExpiresAt,
                LastAttemptPoints = lastAttempt?.Points,
                ProgressPercentage = mastery.ProgressPercentage,
                HasCompletedAttempts = userAttempts.Any(a => a.Status == QuizAttemptStatus.Completed)
            };
        }).ToList();
    }
}
