using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Quiz;

public sealed class GetUserQuizzesQuery : BasePagedQuery<QuizDto>
{
    public Guid? CourseId { get; set; }
}

public class GetUserQuizzesQueryHandler(IUnitOfWork unitOfWork)
    : BasePagedQueryHandler<GetUserQuizzesQuery, Domain.Entities.Quiz, QuizDto>
{
    protected override string SuccessMessage => QuizMessageConsts.GetQuizzesSuccess;

    protected override string ErrorMessage => QuizMessageConsts.GetQuizzesError;

    protected override Task<IQueryable<Domain.Entities.Quiz>> GetQueryableAsync(GetUserQuizzesQuery query, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();

        var queryable = quizRepo.Find(q => q.UserId == query.UserId);

        if (query.CourseId.HasValue)
        {
            queryable = queryable.Where(q => q.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var rawTerm = query.SearchTerm.Trim();
            var normalizedTerm = TextNormalizationHelper.Normalize(rawTerm);

            queryable = queryable.Where(q =>
                EF.Functions.Like(q.Name, $"%{rawTerm}%") ||
                (q.Course != null && EF.Functions.Like(q.Course.Name, $"%{rawTerm}%")) ||
                q.Name.ToLower().Contains(rawTerm.ToLower()) ||
                (q.Course != null && q.Course.Name.ToLower().Contains(rawTerm.ToLower())) ||
                q.Name.ToLower().Contains(normalizedTerm) ||
                (q.Course != null && q.Course.Name.ToLower().Contains(normalizedTerm)));
        }

        queryable = queryable
            .Include(q => q.Course)
            .Include(q => q.Questions)
                .ThenInclude(qn => qn.Options)
            .Include(q => q.Attempts)
                .ThenInclude(a => a.Answers)
                    .ThenInclude(ans => ans.SelectedOptions)
            .OrderByDescending(q => q.CreatedAt);

        return Task.FromResult(queryable);
    }

    protected override QuizDto MapToDto(Domain.Entities.Quiz q, GetUserQuizzesQuery query)
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
    }
}
