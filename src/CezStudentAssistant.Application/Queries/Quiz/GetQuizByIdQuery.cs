using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Quiz;

public sealed class GetQuizByIdQuery : IQuery<QuizDetailsDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
}

public class GetQuizByIdQueryHandler(IUnitOfWork unitOfWork) : BaseQueryHandler<GetQuizByIdQuery, QuizDetailsDto>
{
    protected override string SuccessMessage => QuizMessageConsts.GetQuizSuccess;

    protected override string ErrorMessage => QuizMessageConsts.GetQuizError;

    protected override async Task<QuizDetailsDto> ExecuteAsync(GetQuizByIdQuery query, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();

        var quiz = await quizRepo.Find(q => q.Id == query.QuizId && (q.UserId == query.UserId || q.Course.Users.Any(u => u.Id == query.UserId)))
            .Include(q => q.Course)
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(ct);

        if (quiz == null)
            throw new NotFoundException(QuizMessageConsts.QuizNotFound);

        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var attempts = await quizAttemptRepo.Find(a => a.QuizId == query.QuizId && a.UserId == query.UserId)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOptions)
            .OrderBy(a => a.StartedAt)
            .ToListAsync(ct);

        var mastery = QuizMasteryCalculationHelper.CalculateMastery(quiz.Questions, attempts);

        return new QuizDetailsDto
        {
            Id = quiz.Id,
            UserId = quiz.UserId,
            Name = quiz.Name,
            DisplayName = quiz.DisplayName,
            CourseId = quiz.CourseId,
            CourseName = quiz.Course.Name,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            QuestionCountPerAttempt = quiz.QuestionCountPerAttempt ?? Math.Min(5, quiz.Questions.Count),
            EasyQuestionCountPerAttempt = quiz.EasyQuestionCountPerAttempt,
            MediumQuestionCountPerAttempt = quiz.MediumQuestionCountPerAttempt,
            HardQuestionCountPerAttempt = quiz.HardQuestionCountPerAttempt,
            MaxPoints = quiz.Questions.Sum(qn => qn.Difficulty == QuestionDifficulty.Easy ? 1m : qn.Difficulty == QuestionDifficulty.Hard ? 3m : 2m),
            ProgressPercentage = mastery.ProgressPercentage,
            MasteredQuestionCount = mastery.MasteredCount,
            Questions = quiz.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Content = q.Content,
                Type = q.Type,
                Difficulty = q.Difficulty,
                Options = q.Options.Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList(),
            Attempts = attempts.Select(a =>
            {
                var drawnQuestionIds = a.Answers.Select(ans => ans.QuestionId).ToHashSet();
                var attemptMaxPoints = a.MaxPoints ?? (drawnQuestionIds.Count > 0
                    ? quiz.Questions.Where(q => drawnQuestionIds.Contains(q.Id)).Sum(qn => qn.Difficulty == QuestionDifficulty.Easy ? 1m : qn.Difficulty == QuestionDifficulty.Hard ? 3m : 2m)
                    : quiz.Questions.Sum(qn => qn.Difficulty == QuestionDifficulty.Easy ? 1m : qn.Difficulty == QuestionDifficulty.Hard ? 3m : 2m));

                return new QuizAttemptDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    QuizId = a.QuizId,
                    Status = a.Status,
                    Points = a.Points,
                    MaxPoints = attemptMaxPoints,
                    QuestionCount = a.QuestionCount > 0 ? a.QuestionCount : (a.Answers.Count > 0 ? a.Answers.Count : quiz.Questions.Count),
                    TimeLimitMinutes = a.TimeLimitMinutes,
                    StartedAt = a.StartedAt,
                    ExpiresAt = a.ExpiresAt,
                    Answers = a.Answers.Select(ans => new QuestionAnswerDto
                    {
                        Id = ans.Id,
                        QuestionId = ans.QuestionId,
                        SelectedOptionIds = ans.SelectedOptions.Select(so => so.QuestionOptionId).ToList()
                    }).ToList()
                };
            }).ToList()
        };
    }
}
