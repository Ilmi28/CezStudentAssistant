using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Chat;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Queries.Chat;

public sealed class GetUserCourseChatThreadsQuery : BasePagedQuery<ChatThreadDto>
{
    public Guid? CourseId { get; set; }
}

public class GetUserCourseChatThreadsQueryHandler(IUnitOfWork unitOfWork)
    : BasePagedQueryHandler<GetUserCourseChatThreadsQuery, Domain.Entities.ChatThread, ChatThreadDto>
{
    protected override string SuccessMessage => ChatConsts.GetThreadsSuccess;
    protected override string ErrorMessage => ChatConsts.GetThreadsError;

    protected override Task<IQueryable<Domain.Entities.ChatThread>> GetQueryableAsync(GetUserCourseChatThreadsQuery query, CancellationToken ct)
    {
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();

        var queryable = threadRepo.Find(t => t.UserId == query.UserId);

        if (query.CourseId.HasValue && query.CourseId.Value != Guid.Empty)
        {
            queryable = queryable.Where(t => t.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var rawTerm = query.SearchTerm.Trim();
            var normalizedTerm = TextNormalizationHelper.Normalize(rawTerm);

            queryable = queryable.Where(t =>
                EF.Functions.Like(t.Title, $"%{rawTerm}%") ||
                (t.Course != null && EF.Functions.Like(t.Course.Name, $"%{rawTerm}%")) ||
                t.Title.ToLower().Contains(rawTerm.ToLower()) ||
                (t.Course != null && t.Course.Name.ToLower().Contains(rawTerm.ToLower())) ||
                t.Title.ToLower().Contains(normalizedTerm) ||
                (t.Course != null && t.Course.Name.ToLower().Contains(normalizedTerm)));
        }

        queryable = queryable
            .Include(t => t.Course)
            .Include(t => t.AttachedResources)
            .Include(t => t.Messages)
            .Where(t => t.Messages.Any())
            .OrderByDescending(t => t.LastModifiedAt);

        return Task.FromResult(queryable);
    }

    protected override ChatThreadDto MapToDto(Domain.Entities.ChatThread t, GetUserCourseChatThreadsQuery query)
    {
        var lastMessage = t.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
        return new ChatThreadDto
        {
            Id = t.Id,
            CourseId = t.CourseId,
            CourseName = t.Course?.Name ?? string.Empty,
            UserId = t.UserId,
            Title = t.Title,
            CreatedAt = t.CreatedAt,
            LastMessageAt = lastMessage?.CreatedAt ?? t.CreatedAt,
            LastMessageSnippet = lastMessage?.Content,
            AttachedResourceIds = t.AttachedResources.Select(r => r.Id).ToList()
        };
    }
}
