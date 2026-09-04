using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Queries.Course;

public class GetCourseDetailsQuery : IQuery<CourseDetailsDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
}

public class GetCourseDetailsQueryHandler(IUnitOfWork unitOfWork) : BaseQueryHandler<GetCourseDetailsQuery, CourseDetailsDto>
{
    protected override string SuccessMessage => CourseMessageConsts.GetCourseDetailsSuccess;
    protected override string ErrorMessage => CourseMessageConsts.GetCourseDetailsError;

    protected override async Task<CourseDetailsDto> ExecuteAsync(GetCourseDetailsQuery query, CancellationToken ct)
    {
        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepository.GetByIdAsync(query.CourseId, ct, true, c => c.Users);

        if (course == null)
        {
            throw new NotFoundException(CourseMessageConsts.CourseNotFound);
        }

        if (!course.Users.Any(u => u.Id == query.UserId))
        {
            throw new UnauthorizedException(CourseMessageConsts.CourseAccessDenied);
        }

        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var quizzes = await quizRepo.Find(
            q => q.CourseId == query.CourseId && q.UserId == query.UserId,
            true,
            q => q.Questions,
            q => q.Attempts
        ).ToListAsync(ct);

        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var attemptsWithAnswers = await quizAttemptRepo.Find(
            a => a.UserId == query.UserId && a.Quiz.CourseId == query.CourseId,
            true,
            a => a.Answers
        ).ToListAsync(ct);

        foreach (var quiz in quizzes)
        {
            var quizAttempts = attemptsWithAnswers.Where(a => a.QuizId == quiz.Id).ToList();
            quiz.Attempts = quizAttempts;
        }

        var deckRepo = unitOfWork.Repository<IFlashcardDeckRepository>();
        var decks = await deckRepo.Find(
            d => d.CourseId == query.CourseId && d.UserId == query.UserId,
            true,
            d => d.Cards,
            d => d.Attempts
        ).ToListAsync(ct);

        var prepResult = CoursePreparationCalculationHelper.CalculatePreparation(quizzes, decks);

        var dto = new CourseDetailsDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Type = course.Type,
            LastSynched = course.LastSynched,
            IsCez = course.CezExternalId != null,
            PreparationPercentage = prepResult.PreparationPercentage,
            QuizProgressPercentage = prepResult.QuizProgressPercentage,
            FlashcardProgressPercentage = prepResult.FlashcardProgressPercentage,
            Files = []
        };

        return dto;
    }
}
