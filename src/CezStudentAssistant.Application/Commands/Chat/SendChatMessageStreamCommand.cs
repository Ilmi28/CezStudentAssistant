using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using System.Text;

namespace CezStudentAssistant.Application.Commands.Chat;

public sealed class SendChatMessageStreamCommand : IUserRequest
{
    public Guid UserId { get; set; }
    public Guid ChatThreadId { get; set; }
    public required string UserMessage { get; set; }
}

public class SendChatMessageStreamCommandHandler(
    IUnitOfWork unitOfWork,
    IAIClient aiClient,
    IFileService fileService,
    IConfiguration configuration)
{
    public async IAsyncEnumerable<string> ExecuteStreamAsync(
        SendChatMessageStreamCommand command,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var threadRepo = unitOfWork.Repository<IChatThreadRepository>();
        var thread = await threadRepo.Find(t => t.Id == command.ChatThreadId)
            .Include(t => t.Course)
            .Include(t => t.AttachedResources)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(ChatConsts.ThreadNotFound);

        if (thread.UserId != command.UserId)
        {
            throw new ForbiddenException(ChatConsts.AccessDenied);
        }

        var messageRepo = unitOfWork.Repository<IChatMessageRepository>();
        var history = await messageRepo.Find(m => m.ChatThreadId == command.ChatThreadId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        var isFirstMessage = history.Count == 0;
        if (isFirstMessage && (thread.Title == "Nowy czat" || thread.Title == "Nowy wątek czatu" || string.IsNullOrWhiteSpace(thread.Title)))
        {
            var cleanPrompt = command.UserMessage.Trim();
            thread.Title = cleanPrompt.Length > 35 ? cleanPrompt[..35] + "..." : cleanPrompt;
            await threadRepo.UpdateAsync(thread, ct);
        }

        var userMsg = new ChatMessage
        {
            ChatThreadId = command.ChatThreadId,
            Role = ChatMessageRole.User,
            Content = command.UserMessage.Trim(),
            TokenCount = await aiClient.EstimateTextTokenUsageAsync(command.UserMessage)
        };
        await messageRepo.AddAsync(userMsg, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var aiFiles = new List<AIFile>();
        var streamsToDispose = new List<Stream>();

        try
        {
            if (thread.AttachedResources.Count > 0)
            {
                var containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
                    ?? throw new InvalidOperationException(ChatConsts.CourseFilesContainerMissing);

                foreach (var resource in thread.AttachedResources)
                {
                    var stream = await fileService.DownloadAsync($"{thread.CourseId}/{resource.Name}", containerName, ct);
                    if (stream != null)
                    {
                        streamsToDispose.Add(stream);
                        aiFiles.Add(new AIFile
                        {
                            Stream = stream,
                            MimeType = resource.MimeType
                        });
                    }
                }
            }

            var fullResponseBuilder = new StringBuilder();

            await foreach (var chunk in aiClient.StreamChatResponseAsync(history, command.UserMessage, aiFiles, thread.Course?.Name, ct))
            {
                fullResponseBuilder.Append(chunk);
                yield return chunk;
            }

            var fullResponseText = fullResponseBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(fullResponseText))
            {
                var assistantTokenCount = await aiClient.EstimateTextTokenUsageAsync(fullResponseText);
                var assistantMsg = new ChatMessage
                {
                    ChatThreadId = command.ChatThreadId,
                    Role = ChatMessageRole.Assistant,
                    Content = fullResponseText,
                    TokenCount = assistantTokenCount
                };
                await messageRepo.AddAsync(assistantMsg, ct);

                var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
                var totalTokens = userMsg.TokenCount + assistantTokenCount;

                var startOfDay = DateTime.UtcNow.Date;
                var endOfDay = startOfDay.AddDays(1);

                var existingUsage = await tokenUsageRepo
                    .Find(u => u.UserId == command.UserId && u.UsageType == UsageTokenType.ChatGeneration && u.CreatedAt >= startOfDay && u.CreatedAt < endOfDay)
                    .FirstOrDefaultAsync(ct);

                if (existingUsage != null)
                {
                    existingUsage.UsageCount += totalTokens;
                    await tokenUsageRepo.UpdateAsync(existingUsage, ct);
                }
                else
                {
                    await tokenUsageRepo.AddAsync(new TokenUsage
                    {
                        UserId = command.UserId,
                        UsageType = UsageTokenType.ChatGeneration,
                        UsageCount = totalTokens
                    }, ct);
                }

                if (isFirstMessage)
                {
                    var aiTitle = await aiClient.GenerateChatTitleAsync(command.UserMessage, fullResponseText, ct);
                    if (!string.IsNullOrWhiteSpace(aiTitle))
                    {
                        thread.Title = aiTitle;
                    }
                }

                thread.LastModifiedAt = DateTime.UtcNow;
                await threadRepo.UpdateAsync(thread, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }
        finally
        {
            foreach (var stream in streamsToDispose)
            {
                await stream.DisposeAsync();
            }
        }
    }
}
