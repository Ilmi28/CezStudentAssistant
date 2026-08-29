using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Flashcard;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Flashcard;

public sealed class GetFlashcardDeckByIdQuery : IQuery<FlashcardDeckDetailsDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
}

public class GetFlashcardDeckByIdQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetFlashcardDeckByIdQuery, FlashcardDeckDetailsDto>
{
    protected override string SuccessMessage => FlashcardMessageConsts.GetFlashcardDeckSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.GetFlashcardDeckSuccess;

    protected override async Task<FlashcardDeckDetailsDto> ExecuteAsync(GetFlashcardDeckByIdQuery query, CancellationToken ct)
    {
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();

        var deck = await deckRepo.Find(d => d.Id == query.DeckId)
            .Include(d => d.Course)
            .Include(d => d.Cards)
            .Include(d => d.Attempts).ThenInclude(a => a.Cards)
            .FirstOrDefaultAsync(ct);

        if (deck == null)
        {
            throw new NotFoundException(FlashcardMessageConsts.FlashcardDeckNotFound);
        }

        if (deck.UserId != query.UserId)
        {
            throw new UnauthorizedException(FlashcardMessageConsts.FlashcardAccessDenied);
        }

        var cardList = deck.Cards.OrderBy(c => c.CreatedAt).ToList();
        var mastered = cardList.Count(c => c.State == FlashcardStateEnum.Mastered);
        var learning = cardList.Count(c => c.State == FlashcardStateEnum.Learning);
        var newCards = cardList.Count(c => c.State == FlashcardStateEnum.New);

        return new FlashcardDeckDetailsDto
        {
            Id = deck.Id,
            UserId = deck.UserId,
            Name = deck.Name,
            CourseId = deck.CourseId,
            CourseName = deck.Course?.Name ?? string.Empty,
            Status = deck.Status,
            CardCountPerAttempt = deck.CardCountPerAttempt ?? Math.Min(10, cardList.Count),
            EasyCardCountPerAttempt = deck.EasyCardCountPerAttempt,
            MediumCardCountPerAttempt = deck.MediumCardCountPerAttempt,
            HardCardCountPerAttempt = deck.HardCardCountPerAttempt,
            CardCount = cardList.Count,
            MasteredCardCount = mastered,
            LearningCardCount = learning,
            NewCardCount = newCards,
            ProgressPercentage = FlashcardProgressCalculationHelper.CalculateProgressPercentage(cardList),
            Cards = cardList.Select(c => new FlashcardDto
            {
                Id = c.Id,
                Front = c.Front,
                Back = c.Back,
                Difficulty = c.Difficulty,
                State = c.State,
                DeckId = c.DeckId
            }).ToList(),
            Attempts = deck.Attempts.OrderByDescending(a => a.StartedAt).Select(a =>
            {
                var masteredCount = a.Cards.Count(c => c.State == FlashcardStateEnum.Mastered);
                var learningCount = a.Cards.Count(c => c.State == FlashcardStateEnum.Learning);
                var progressPercentage = FlashcardProgressCalculationHelper.CalculateAttemptProgressPercentage(masteredCount, learningCount, a.CardCount);

                return new FlashcardAttemptDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    DeckId = a.DeckId,
                    Status = a.Status,
                    CardCount = a.CardCount,
                    MasteredCount = masteredCount,
                    LearningCount = learningCount,
                    ProgressPercentage = progressPercentage,
                    CardStates = a.Cards.ToDictionary(c => c.FlashcardId.ToString(), c => c.State),
                    StartedAt = a.StartedAt,
                    CompletedAt = a.CompletedAt
                };
            }).ToList()
        };
    }
}
