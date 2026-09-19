using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Chat;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Queries.Chat;

public sealed class GetChatThreadByIdQuery : IQuery<ChatThreadDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid ChatThreadId { get; set; }
}

public class GetChatThreadByIdQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetChatThreadByIdQuery, ChatThreadDto>
{
    protected override string SuccessMessage => ChatConsts.GetThreadsSuccess;
    protected override string ErrorMessage => ChatConsts.GetThreadsError;

    protected override async Task<ChatThreadDto> ExecuteAsync(GetChatThreadByIdQuery query, CancellationToken ct)
    {
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();
        var thread = await threadRepo.Find(t => t.Id == query.ChatThreadId)
            .Include(t => t.Course)
            .Include(t => t.AttachedResources)
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(ChatConsts.ThreadNotFound);

        if (thread.UserId != query.UserId)
        {
            throw new ForbiddenException(ChatConsts.AccessDenied);
        }

        var lastMessage = thread.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

        return new ChatThreadDto
        {
            Id = thread.Id,
            CourseId = thread.CourseId,
            CourseName = thread.Course?.Name ?? string.Empty,
            UserId = thread.UserId,
            Title = thread.Title,
            CreatedAt = thread.CreatedAt,
            LastMessageAt = lastMessage?.CreatedAt ?? thread.CreatedAt,
            LastMessageSnippet = lastMessage?.Content,
            AttachedResourceIds = thread.AttachedResources.Select(r => r.Id).ToList()
        };
    }
}
