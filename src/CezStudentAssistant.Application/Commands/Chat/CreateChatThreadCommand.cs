using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Chat;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;

using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Commands.Chat;

public sealed class CreateChatThreadCommand : ICommand<ChatThreadDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public List<Guid>? AttachedResourceIds { get; set; }
}

public class CreateChatThreadCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<CreateChatThreadCommand, ChatThreadDto>
{
    protected override string SuccessMessage => ChatConsts.ThreadCreatedSuccess;
    protected override string ErrorMessage => ChatConsts.ThreadCreatedError;

    protected override async Task<ChatThreadDto> ExecuteAsync(CreateChatThreadCommand command, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetByIdAsync(command.CourseId, ct)
            ?? throw new NotFoundException(AIMessageConsts.CourseNotFound);

        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();

        var existingEmptyThread = await threadRepo.Find(t =>
            t.UserId == command.UserId &&
            t.CourseId == command.CourseId &&
            !t.Messages.Any())
            .Include(t => t.AttachedResources)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (existingEmptyThread != null)
        {
            if (command.AttachedResourceIds != null)
            {
                existingEmptyThread.AttachedResources.Clear();
                if (command.AttachedResourceIds.Count > 0)
                {
                    var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
                    var resources = await resourceRepo.Find(r => command.AttachedResourceIds.Contains(r.Id) && r.CourseId == command.CourseId).ToListAsync(ct);
                    foreach (var resource in resources)
                    {
                        existingEmptyThread.AttachedResources.Add(resource);
                    }
                }
                await unitOfWork.SaveChangesAsync(ct);
            }

            return new ChatThreadDto
            {
                Id = existingEmptyThread.Id,
                CourseId = existingEmptyThread.CourseId,
                CourseName = course.Name,
                UserId = existingEmptyThread.UserId,
                Title = existingEmptyThread.Title,
                CreatedAt = existingEmptyThread.CreatedAt,
                LastMessageAt = existingEmptyThread.CreatedAt,
                LastMessageSnippet = null,
                AttachedResourceIds = existingEmptyThread.AttachedResources.Select(r => r.Id).ToList()
            };
        }

        var thread = new ChatThread
        {
            Title = "Nowy czat",
            CourseId = command.CourseId,
            UserId = command.UserId
        };

        if (command.AttachedResourceIds != null)
        {
            if (command.AttachedResourceIds.Count > 0)
            {
                var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
                var resources = await resourceRepo.Find(r => command.AttachedResourceIds.Contains(r.Id) && r.CourseId == command.CourseId).ToListAsync(ct);
                foreach (var resource in resources)
                {
                    thread.AttachedResources.Add(resource);
                }
            }
        }
        else
        {
            var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
            var defaultResources = await resourceRepo.Find(r => r.CourseId == command.CourseId && !r.IsHidden).ToListAsync(ct);
            foreach (var resource in defaultResources)
            {
                thread.AttachedResources.Add(resource);
            }
        }

        await threadRepo.AddAsync(thread, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new ChatThreadDto
        {
            Id = thread.Id,
            CourseId = thread.CourseId,
            CourseName = course.Name,
            UserId = thread.UserId,
            Title = thread.Title,
            CreatedAt = thread.CreatedAt,
            LastMessageAt = thread.CreatedAt,
            LastMessageSnippet = null,
            AttachedResourceIds = thread.AttachedResources.Select(r => r.Id).ToList()
        };
    }
}
