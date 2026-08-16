using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
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

        return new QuizDetailsDto
        {
            Id = quiz.Id,
            UserId = quiz.UserId,
            Name = quiz.Name,
            DisplayName = quiz.DisplayName,
            CourseId = quiz.CourseId,
            CourseName = quiz.Course.Name,
            Questions = quiz.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Content = q.Content,
                Type = q.Type,
                Difficulty = q.Difficulty,
                Points = q.Points,
                Options = q.Options.Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList()
        };
    }
}
