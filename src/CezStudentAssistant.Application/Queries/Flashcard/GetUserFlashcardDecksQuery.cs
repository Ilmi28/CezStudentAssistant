using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Flashcard;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Flashcard;

public sealed class GetUserFlashcardDecksQuery : BasePagedQuery<FlashcardDeckDto>
{
    public Guid? CourseId { get; set; }
}

public class GetUserFlashcardDecksQueryHandler(IUnitOfWork unitOfWork)
    : BasePagedQueryHandler<GetUserFlashcardDecksQuery, FlashcardDeck, FlashcardDeckDto>
{
    protected override string SuccessMessage => FlashcardMessageConsts.GetFlashcardDecksSuccess;

    protected override string ErrorMessage => FlashcardMessageConsts.GetFlashcardDecksSuccess;

    protected override Task<IQueryable<FlashcardDeck>> GetQueryableAsync(GetUserFlashcardDecksQuery query, CancellationToken ct)
    {
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();

        var queryable = deckRepo.Find(d => d.UserId == query.UserId);

        if (query.CourseId.HasValue)
        {
            queryable = queryable.Where(d => d.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var rawTerm = query.SearchTerm.Trim();
            var normalizedTerm = TextNormalizationHelper.Normalize(rawTerm);

            queryable = queryable.Where(d =>
                EF.Functions.Like(d.Name, $"%{rawTerm}%") ||
                (d.Course != null && EF.Functions.Like(d.Course.Name, $"%{rawTerm}%")) ||
                d.Name.ToLower().Contains(rawTerm.ToLower()) ||
                (d.Course != null && d.Course.Name.ToLower().Contains(rawTerm.ToLower())) ||
                d.Name.ToLower().Contains(normalizedTerm) ||
                (d.Course != null && d.Course.Name.ToLower().Contains(normalizedTerm)));
        }

        queryable = queryable
            .Include(d => d.Course)
            .Include(d => d.Cards)
            .Include(d => d.Attempts).ThenInclude(a => a.Cards)
            .OrderByDescending(d => d.CreatedAt);

        return Task.FromResult(queryable);
    }

    protected override FlashcardDeckDto MapToDto(FlashcardDeck d, GetUserFlashcardDecksQuery query)
    {
        var cardList = d.Cards.ToList();
        var attemptsList = d.Attempts.ToList();
        var mastered = cardList.Count(c => FlashcardProgressCalculationHelper.IsCardMastered(c, attemptsList));
        var learning = cardList.Count(c => !FlashcardProgressCalculationHelper.IsCardMastered(c, attemptsList) && c.State == FlashcardStateEnum.Learning);
        var newCards = cardList.Count(c => !FlashcardProgressCalculationHelper.IsCardMastered(c, attemptsList) && c.State == FlashcardStateEnum.New);

        return new FlashcardDeckDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Name = d.Name,
            CourseId = d.CourseId,
            CourseName = d.Course?.Name ?? string.Empty,
            Status = d.Status,
            CardCount = cardList.Count,
            MasteredCardCount = mastered,
            LearningCardCount = learning,
            NewCardCount = newCards,
            ProgressPercentage = FlashcardProgressCalculationHelper.CalculateDeckProgressPercentage(cardList, attemptsList)
        };
    }
}
