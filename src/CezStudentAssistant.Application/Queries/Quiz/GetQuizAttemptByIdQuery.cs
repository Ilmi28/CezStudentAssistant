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

public sealed class GetQuizAttemptByIdQuery : IQuery<QuizAttemptDetailsDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizAttemptId { get; set; }
}

public class GetQuizAttemptByIdQueryHandler(IUnitOfWork unitOfWork) : BaseQueryHandler<GetQuizAttemptByIdQuery, QuizAttemptDetailsDto>
{
    protected override string SuccessMessage => QuizMessageConsts.GetQuizAttemptSuccess;

    protected override string ErrorMessage => QuizMessageConsts.GetQuizAttemptError;

    protected override async Task<QuizAttemptDetailsDto> ExecuteAsync(GetQuizAttemptByIdQuery query, CancellationToken ct)
    {
        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();

        var quizAttempt = await quizAttemptRepo.Find(a => a.Id == query.QuizAttemptId && a.UserId == query.UserId)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Course)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Options)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOptions)
            .FirstOrDefaultAsync(ct);

        if (quizAttempt == null)
            throw new NotFoundException(QuizMessageConsts.QuizAttemptNotFound);

        var isPending = quizAttempt.Status == QuizAttemptStatus.InProgress &&
            (quizAttempt.ExpiresAt == null || quizAttempt.ExpiresAt > DateTime.UtcNow);

        return new QuizAttemptDetailsDto
        {
            AttemptId = quizAttempt.Id,
            QuizId = quizAttempt.QuizId,
            DisplayName = quizAttempt.Quiz.DisplayName,
            CourseName = quizAttempt.Quiz.Course.Name,
            Status = quizAttempt.Status,
            IsPending = isPending,
            Points = quizAttempt.Points,
            StartedAt = quizAttempt.StartedAt,
            ExpiresAt = quizAttempt.ExpiresAt,
            Questions = quizAttempt.Quiz.Questions.Select(q => new QuestionDto
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
            Answers = quizAttempt.Answers.Select(ans => new QuestionAnswerDto
            {
                Id = ans.Id,
                QuestionId = ans.QuestionId,
                SelectedOptionIds = ans.SelectedOptions.Select(so => so.QuestionOptionId).ToList()
            }).ToList()
        };
    }
}
