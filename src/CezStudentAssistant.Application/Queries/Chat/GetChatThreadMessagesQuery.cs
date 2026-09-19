using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Chat;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Queries.Chat;

public sealed class GetChatThreadMessagesQuery : IQuery<List<ChatMessageDto>>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid ChatThreadId { get; set; }
}

public class GetChatThreadMessagesQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetChatThreadMessagesQuery, List<ChatMessageDto>>
{
    protected override string SuccessMessage => ChatConsts.GetMessagesSuccess;
    protected override string ErrorMessage => ChatConsts.GetMessagesError;

    protected override async Task<List<ChatMessageDto>> ExecuteAsync(GetChatThreadMessagesQuery query, CancellationToken ct)
    {
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();
        var thread = await threadRepo.GetByIdAsync(query.ChatThreadId, ct)
            ?? throw new NotFoundException(ChatConsts.ThreadNotFound);

        if (thread.UserId != query.UserId)
        {
            throw new ForbiddenException(ChatConsts.AccessDenied);
        }

        var messageRepo = unitOfWork.Repository<IChatMessageRepository>();
        var messages = await messageRepo.Find(m => m.ChatThreadId == query.ChatThreadId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ChatThreadId = m.ChatThreadId,
            Role = m.Role,
            Content = m.Content,
            TokenCount = m.TokenCount,
            CreatedAt = m.CreatedAt
        }).ToList();
    }
}
