using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.User;

public sealed class GetDashboardStatsQuery : IQuery<DashboardStatsDto>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    protected override string SuccessMessage => UserMessageConsts.GetDashboardStatsSuccess;
    protected override string ErrorMessage => UserMessageConsts.GetDashboardStatsError;

    protected override async Task<DashboardStatsDto> ExecuteAsync(GetDashboardStatsQuery query, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var cardRepo = unitOfWork.Repository<IFlashcardRepository>();

        var courseCount = await courseRepo
            .Find(c => c.Users.Any(u => u.Id == query.UserId), asNoTracking: true)
            .CountAsync(ct);

        var quizCount = await quizRepo
            .Find(q => q.UserId == query.UserId, asNoTracking: true)
            .CountAsync(ct);

        var flashcardDeckCount = await deckRepo
            .Find(d => d.UserId == query.UserId, asNoTracking: true)
            .CountAsync(ct);

        var flashcardCount = await cardRepo
            .Find(c => c.Deck.UserId == query.UserId, asNoTracking: true)
            .CountAsync(ct);

        return new DashboardStatsDto
        {
            CourseCount = courseCount,
            QuizCount = quizCount,
            FlashcardDeckCount = flashcardDeckCount,
            FlashcardCount = flashcardCount
        };
    }
}
