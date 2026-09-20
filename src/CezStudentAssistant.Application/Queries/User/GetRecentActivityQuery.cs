using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.User;

public sealed class GetRecentActivityQuery : IQuery<List<RecentActivityDto>>, IUserRequest
{
    public Guid UserId { get; set; }
    public int Limit { get; set; } = 6;
}

public class GetRecentActivityQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetRecentActivityQuery, List<RecentActivityDto>>
{
    protected override string SuccessMessage => UserMessageConsts.GetRecentActivitySuccess;
    protected override string ErrorMessage => UserMessageConsts.GetRecentActivityError;

    protected override async Task<List<RecentActivityDto>> ExecuteAsync(GetRecentActivityQuery query, CancellationToken ct)
    {
        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var flashcardAttemptRepo = unitOfWork.Repository<IFlashcardAttemptRepository>();
        var chatThreadRepo = unitOfWork.Repository<IChatThreadRepository>();

        var recentQuizAttempts = await quizAttemptRepo
            .Find(q => q.UserId == query.UserId, asNoTracking: true, includes: [x => x.Quiz, x => x.Course, x => x.Answers])
            .ToListAsync(ct);

        var recentFlashcardAttempts = await flashcardAttemptRepo
            .Find(f => f.UserId == query.UserId, asNoTracking: true, includes: [x => x.Deck, x => x.Deck.Course, x => x.Deck.Cards, x => x.Cards])
            .ToListAsync(ct);

        var recentChatThreads = await chatThreadRepo
            .Find(t => t.UserId == query.UserId, asNoTracking: true, includes: [x => x.Course, x => x.Messages])
            .ToListAsync(ct);

        var quizActivities = recentQuizAttempts.Select(qa =>
        {
            double? score = null;
            if (qa.MaxPoints.HasValue && qa.MaxPoints.Value > 0 && qa.Points.HasValue)
            {
                score = Math.Round((double)(qa.Points.Value / qa.MaxPoints.Value * 100), 2);
            }

            var dates = new List<DateTime> { qa.StartedAt, qa.LastModifiedAt };
            if (qa.Answers != null && qa.Answers.Count > 0)
            {
                dates.AddRange(qa.Answers.Select(a => a.LastModifiedAt != default ? a.LastModifiedAt : a.CreatedAt));
            }
            var lastActivityDate = dates.Max();

            return new RecentActivityDto
            {
                Id = qa.Id,
                EntityId = qa.QuizId,
                Type = ActivityType.Quiz,
                Title = qa.Quiz?.Name ?? string.Empty,
                CourseName = qa.Course?.Name ?? string.Empty,
                ScorePercentage = score,
                EarnedPoints = qa.Points,
                MaxPoints = qa.MaxPoints,
                AttemptDate = lastActivityDate,
                Status = qa.Status.ToString()
            };
        });

        var flashcardActivities = recentFlashcardAttempts.Select(fa =>
        {
            int total = fa.CardCount > 0 ? fa.CardCount : (fa.Deck?.Cards?.Count ?? fa.Cards?.Count ?? 0);
            int learnedCount = fa.Cards != null ? fa.Cards.Count(c => c.State == FlashcardStateEnum.Mastered || c.State == FlashcardStateEnum.Learning) : 0;
            double? score = total > 0 ? Math.Round((double)learnedCount / total * 100, 2) : 0;

            var dates = new List<DateTime> { fa.StartedAt, fa.LastModifiedAt };
            if (fa.CompletedAt.HasValue)
            {
                dates.Add(fa.CompletedAt.Value);
            }
            if (fa.Cards != null && fa.Cards.Count > 0)
            {
                dates.AddRange(fa.Cards.Select(c => c.LastModifiedAt != default ? c.LastModifiedAt : c.CreatedAt));
            }
            var lastActivityDate = dates.Max();

            return new RecentActivityDto
            {
                Id = fa.Id,
                EntityId = fa.DeckId,
                Type = ActivityType.Flashcard,
                Title = fa.Deck?.Name ?? string.Empty,
                CourseName = fa.Deck?.Course?.Name ?? string.Empty,
                ScorePercentage = score,
                MasteredCount = learnedCount,
                TotalCount = total,
                AttemptDate = lastActivityDate,
                Status = fa.Status.ToString()
            };
        });

        var chatActivities = recentChatThreads.Select(ct =>
        {
            var lastMessage = ct.Messages?.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var lastActivityDate = lastMessage?.CreatedAt ?? (ct.LastModifiedAt != default ? ct.LastModifiedAt : ct.CreatedAt);
            var messageCount = ct.Messages?.Count ?? 0;

            return new RecentActivityDto
            {
                Id = ct.Id,
                EntityId = ct.Id,
                Type = ActivityType.Chat,
                Title = string.IsNullOrWhiteSpace(ct.Title) ? "Nowy wątek czatu" : ct.Title,
                CourseName = ct.Course?.Name ?? string.Empty,
                TotalCount = messageCount,
                AttemptDate = lastActivityDate,
                Status = "Completed"
            };
        });

        var combinedActivities = quizActivities
            .Concat(flashcardActivities)
            .Concat(chatActivities)
            .GroupBy(a => a.EntityId)
            .Select(g => g.OrderByDescending(a => a.AttemptDate).First())
            .OrderByDescending(a => a.AttemptDate)
            .Take(query.Limit)
            .ToList();

        return combinedActivities;
    }
}
