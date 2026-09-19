using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Chat;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Commands.Chat;

public sealed class UpdateChatThreadResourcesCommand : ICommand<ChatThreadDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid ChatThreadId { get; set; }
    public List<Guid> ResourceIds { get; set; } = [];
}

public class UpdateChatThreadResourcesCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<UpdateChatThreadResourcesCommand, ChatThreadDto>
{
    protected override string SuccessMessage => ChatConsts.ThreadUpdatedSuccess;
    protected override string ErrorMessage => ChatConsts.ThreadUpdatedError;

    protected override async Task<ChatThreadDto> ExecuteAsync(UpdateChatThreadResourcesCommand command, CancellationToken ct)
    {
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();
        var thread = await threadRepo.Find(t => t.Id == command.ChatThreadId)
            .Include(t => t.Course)
            .Include(t => t.AttachedResources)
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(ChatConsts.ThreadNotFound);

        if (thread.UserId != command.UserId)
        {
            throw new ForbiddenException(ChatConsts.AccessDenied);
        }

        thread.AttachedResources.Clear();

        if (command.ResourceIds.Count > 0)
        {
            var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
            var resources = await resourceRepo.Find(r => command.ResourceIds.Contains(r.Id) && r.CourseId == thread.CourseId).ToListAsync(ct);
            foreach (var resource in resources)
            {
                thread.AttachedResources.Add(resource);
            }
        }

        await threadRepo.UpdateAsync(thread, ct);
        await unitOfWork.SaveChangesAsync(ct);

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
