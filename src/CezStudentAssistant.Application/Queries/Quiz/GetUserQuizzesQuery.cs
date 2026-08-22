using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Quiz;

public sealed class GetUserQuizzesQuery : IQuery<List<QuizDto>>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetUserQuizzesQueryHandler(IUnitOfWork unitOfWork) : BaseQueryHandler<GetUserQuizzesQuery, List<QuizDto>>
{
    protected override string SuccessMessage => QuizMessageConsts.GetQuizzesSuccess;

    protected override string ErrorMessage => QuizMessageConsts.GetQuizzesError;

    protected override async Task<List<QuizDto>> ExecuteAsync(GetUserQuizzesQuery query, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();

        return await quizRepo.Find(q => q.UserId == query.UserId)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                UserId = q.UserId,
                Name = q.Name,
                DisplayName = q.DisplayName,
                CourseId = q.CourseId,
                CourseName = q.Course.Name,
                Status = q.Status,
                TimeLimitMinutes = q.TimeLimitMinutes
            })
            .ToListAsync(ct);
    }
}
