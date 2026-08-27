using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Flashcard;

public sealed class GenerateFlashcardsCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int CardCount { get; set; } = 10;
    public string? AdditionalInstructions { get; set; }
    public int? EasyCount { get; set; }
    public int? MediumCount { get; set; }
    public int? HardCount { get; set; }
}

public class GenerateFlashcardsCommandHandler(
    IJobScheduler jobScheduler,
    IJobService jobService,
    IUnitOfWork unitOfWork) : BaseCommandHandler<GenerateFlashcardsCommand>
{
    private const int EstimatedTokensPerOutputCard = 150;

    protected override string SuccessMessage => FlashcardMessageConsts.GenerateFlashcardsSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.GenerateFlashcardsError;

    protected override async Task ExecuteAsync(GenerateFlashcardsCommand command, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetByIdAsync(command.CourseId, ct)
            ?? throw new NotFoundException(AIMessageConsts.CourseNotFound);

        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var existingDecks = deckRepo.Find(d => d.CourseId == command.CourseId).ToList();
        var deckTitle = $"Fiszki #{existingDecks.Count + 1}";

        var reservedCount = await EstimateTokensAsync(command.CourseId, command.CardCount, ct);

        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        var reservation = new TokenUsage
        {
            UserId = command.UserId,
            UsageType = UsageTokenType.ReservedCardGeneration,
            UsageCount = reservedCount
        };
        await tokenUsageRepo.AddAsync(reservation, ct);

        var deck = new FlashcardDeck
        {
            UserId = command.UserId,
            Name = deckTitle,
            CourseId = command.CourseId,
            Status = FlashcardDeckStatusEnum.Generating
        };

        await deckRepo.AddAsync(deck, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var job = await jobService.CreateJobAsync(command.UserId, JobType.FlashcardGeneration, ct);

        var dto = new GenerateFlashcardsDto
        {
            DeckId = deck.Id,
            UserId = command.UserId,
            CourseId = command.CourseId,
            ReservationId = reservation.Id,
            CardCount = command.CardCount,
            Language = QuizLanguage.PL,
            AdditionalInstructions = command.AdditionalInstructions,
            EasyCount = command.EasyCount,
            MediumCount = command.MediumCount,
            HardCount = command.HardCount
        };

        var jobId = jobScheduler.Enqueue<IFlashcardGenerationService>(service => service.GenerateFlashcards(dto, ct));
        await jobService.UpdateJobAsync(job, JobStatus.Enqueued, jobId, ct);
    }

    private async Task<int> EstimateTokensAsync(Guid courseId, int cardCount, CancellationToken ct)
    {
        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var resources = await resourceRepo.Find(r => r.CourseId == courseId).ToListAsync(ct);
        var inputTokens = 0;

        foreach (var resource in resources)
        {
            inputTokens += resource.EstimatedTokens;
        }

        var outputTokens = Math.Max(1, cardCount) * EstimatedTokensPerOutputCard;
        return inputTokens + outputTokens;
    }
}
