using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Flashcard;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Flashcard;

public sealed class GetUserFlashcardDecksQuery : IQuery<List<FlashcardDeckDto>>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid? CourseId { get; set; }
}

public class GetUserFlashcardDecksQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetUserFlashcardDecksQuery, List<FlashcardDeckDto>>
{
    protected override string SuccessMessage => FlashcardMessageConsts.GetFlashcardDecksSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.GetFlashcardDecksSuccess;

    protected override async Task<List<FlashcardDeckDto>> ExecuteAsync(GetUserFlashcardDecksQuery query, CancellationToken ct)
    {
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();

        var queryable = deckRepo.Find(d => d.UserId == query.UserId)
            .Include(d => d.Course)
            .Include(d => d.Cards)
            .Include(d => d.Attempts).ThenInclude(a => a.Cards);

        if (query.CourseId.HasValue)
        {
            queryable = queryable.Where(d => d.CourseId == query.CourseId.Value)
                .Include(d => d.Course)
                .Include(d => d.Cards)
                .Include(d => d.Attempts).ThenInclude(a => a.Cards);
        }

        var decks = await queryable.OrderByDescending(d => d.CreatedAt).ToListAsync(ct);

        return decks.Select(d =>
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
        }).ToList();
    }
}
