using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Flashcard;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Services;

public class FlashcardGenerationService(
    IAIClient aiClient,
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IJobService jobService,
    IConfiguration configuration) : IFlashcardGenerationService, IScopedService
{
    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? throw new InvalidOperationException(CourseMessageConsts.CourseFilesContainerConfigMissing);

    public async Task GenerateFlashcards(GenerateFlashcardsDto dto, CancellationToken ct = default)
    {
        var job = await jobService.GetLatestJobAsync(dto.UserId, JobType.FlashcardGeneration, ct)
            ?? throw new AppException(AIMessageConsts.JobNotFound);

        await jobService.UpdateJobAsync(job, JobStatus.Processing, ct: ct);

        if (dto.DeckId != Guid.Empty)
        {
            var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
            var deck = await deckRepo.GetByIdAsync(dto.DeckId, ct);
            if (deck != null && deck.Status != FlashcardDeckStatusEnum.Generating)
            {
                deck.Status = FlashcardDeckStatusEnum.Generating;
                await deckRepo.UpdateAsync(deck, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }

        var aiFiles = new List<AIFile>();
        try
        {
            var maxTokensConfig = configuration["Gemini:MaximumDailyTokens"];
            if (string.IsNullOrWhiteSpace(maxTokensConfig) || !int.TryParse(maxTokensConfig, out var dailyTokenLimit) || dailyTokenLimit <= 0)
            {
                throw new InvalidOperationException(UserMessageConsts.MaximumDailyTokensConfigMissing);
            }

            var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
            var dailyTokensUsed = await tokenUsageRepo.GetDailyTokenUsageAsync(dto.UserId, DateTime.UtcNow, ct);

            aiFiles = await DownloadCourseFilesAsync(dto.CourseId, ct);

            var aiRequest = new AIFlashcardRequest
            {
                CardCount = dto.CardCount,
                Language = dto.Language,
                Files = aiFiles,
                AdditionalInstructions = dto.AdditionalInstructions,
                EasyCount = dto.EasyCount,
                MediumCount = dto.MediumCount,
                HardCount = dto.HardCount
            };

            var estimatedTokens = await aiClient.EstimateTokenUsageAsync(new AIQuizRequest
            {
                QuestionCount = 1,
                Files = aiFiles
            });
            var estimatedOutputTokens = Math.Max(1, dto.CardCount) * 150;
            var totalEstimatedTokens = estimatedTokens + estimatedOutputTokens;

            if (dailyTokensUsed + totalEstimatedTokens > dailyTokenLimit)
            {
                throw new BadRequestException(AIMessageConsts.DailyTokenLimitExceeded);
            }

            var parsedDeck = await aiClient.GenerateFlashcardsAsync(aiRequest);

            if (parsedDeck == null || !parsedDeck.Cards.Any())
            {
                throw new BadRequestException(FlashcardMessageConsts.GenerateFlashcardsError);
            }

            await CommitTokenUsageAsync(dto.UserId, dto.ReservationId, totalEstimatedTokens, ct);
            await SaveDeckWithCardsAsync(dto, parsedDeck, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await jobService.UpdateJobAsync(job, JobStatus.Succeeded, ct: ct);
        }
        catch
        {
            await ReleaseTokenReservationAsync(dto.ReservationId, ct);
            await MarkDeckAsFailedAsync(dto.DeckId, ct);
            await jobService.UpdateJobAsync(job, JobStatus.Failed, ct: ct);
            throw;
        }
        finally
        {
            DisposeFiles(aiFiles);
        }
    }

    private async Task<List<AIFile>> DownloadCourseFilesAsync(Guid courseId, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetByIdAsync(courseId, ct)
            ?? throw new NotFoundException(AIMessageConsts.CourseNotFound);

        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var resources = await resourceRepo.Find(r => r.CourseId == courseId && !r.IsHidden).ToListAsync(ct);

        var files = new List<AIFile>();
        foreach (var resource in resources)
        {
            var stream = await fileService.DownloadAsync($"{courseId}/{resource.Name}", _containerName, ct);
            files.Add(new AIFile { Stream = stream, MimeType = resource.MimeType });
        }
        return files;
    }

    private async Task CommitTokenUsageAsync(Guid userId, Guid reservationId, int tokenCount, CancellationToken ct)
    {
        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        if (reservationId != Guid.Empty)
        {
            var reservation = await tokenUsageRepo.GetByIdAsync(reservationId, ct);
            if (reservation != null)
            {
                reservation.UsageType = UsageTokenType.CardGeneration;
                reservation.UsageCount = tokenCount;
                return;
            }
        }

        await tokenUsageRepo.AddAsync(new TokenUsage
        {
            UserId = userId,
            UsageType = UsageTokenType.CardGeneration,
            UsageCount = tokenCount
        }, ct);
    }

    private async Task ReleaseTokenReservationAsync(Guid reservationId, CancellationToken ct)
    {
        if (reservationId == Guid.Empty) return;
        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        var reservation = await tokenUsageRepo.GetByIdAsync(reservationId, ct);
        if (reservation != null)
        {
            await tokenUsageRepo.DeleteAsync(reservation, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private async Task SaveDeckWithCardsAsync(GenerateFlashcardsDto dto, AIFlashcardDeck aiDeck, CancellationToken ct)
    {
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var cardRepo = unitOfWork.Repository<IFlashcardRepository>();

        var deck = dto.DeckId != Guid.Empty ? await deckRepo.GetByIdAsync(dto.DeckId, ct) : null;

        if (deck == null)
        {
            var existingDecks = deckRepo.Find(d => d.CourseId == dto.CourseId).ToList();
            var deckNumberTitle = $"Fiszki #{existingDecks.Count + 1}";
            var finalTitle = BuildDeckTitle(deckNumberTitle, aiDeck.Title);

            deck = new FlashcardDeck
            {
                UserId = dto.UserId,
                Name = finalTitle,
                CourseId = dto.CourseId,
                Status = FlashcardDeckStatusEnum.Ready
            };
            await deckRepo.AddAsync(deck, ct);
            foreach (var aiCard in aiDeck.Cards)
            {
                await cardRepo.AddAsync(new Flashcard
                {
                    DeckId = deck.Id,
                    Front = aiCard.Front,
                    Back = aiCard.Back,
                    Difficulty = aiCard.Difficulty,
                    State = FlashcardStateEnum.New
                }, ct);
            }
        }
        else
        {
            var deckNumberTitle = !string.IsNullOrWhiteSpace(deck.Name) && deck.Name.StartsWith("Fiszki #", StringComparison.OrdinalIgnoreCase)
                ? deck.Name
                : $"Fiszki #{deckRepo.Find(d => d.CourseId == dto.CourseId).Count()}";

            var finalTitle = BuildDeckTitle(deckNumberTitle, aiDeck.Title);

            deck.Name = finalTitle;
            deck.Status = FlashcardDeckStatusEnum.Ready;
            foreach (var aiCard in aiDeck.Cards)
            {
                await cardRepo.AddAsync(new Flashcard
                {
                    DeckId = deck.Id,
                    Front = aiCard.Front,
                    Back = aiCard.Back,
                    Difficulty = aiCard.Difficulty,
                    State = FlashcardStateEnum.New
                }, ct);
            }
        }
    }

    private static string BuildDeckTitle(string deckNumberTitle, string? aiTitle)
    {
        if (string.IsNullOrWhiteSpace(aiTitle))
            return deckNumberTitle;

        var topic = aiTitle.Trim();
        if (topic.EndsWith(" - Fiszki", StringComparison.OrdinalIgnoreCase))
        {
            topic = topic[..^9].Trim();
        }
        if (topic.StartsWith("Fiszki z ", StringComparison.OrdinalIgnoreCase))
        {
            topic = topic[9..].Trim();
        }

        if (string.IsNullOrWhiteSpace(topic) ||
            topic.Equals("Fiszki", StringComparison.OrdinalIgnoreCase) ||
            topic.Equals(deckNumberTitle, StringComparison.OrdinalIgnoreCase) ||
            topic.StartsWith("Fiszki #", StringComparison.OrdinalIgnoreCase))
        {
            return deckNumberTitle;
        }

        return $"{deckNumberTitle} - {topic}";
    }

    private async Task MarkDeckAsFailedAsync(Guid deckId, CancellationToken ct)
    {
        if (deckId == Guid.Empty) return;

        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var deck = await deckRepo.GetByIdAsync(deckId, ct);
        if (deck != null)
        {
            deck.Status = FlashcardDeckStatusEnum.Failed;
            await deckRepo.UpdateAsync(deck, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private static void DisposeFiles(IEnumerable<AIFile> files)
    {
        foreach (var file in files)
        {
            file.Stream?.Dispose();
        }
    }
}
