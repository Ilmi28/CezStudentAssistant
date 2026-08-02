using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Queries.Course;

public sealed class GetUserCoursesQuery : IQuery<List<CourseDto>>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetUserCoursesQueryHandler(IUnitOfWork unitOfWork) : BaseQueryHandler<GetUserCoursesQuery, List<CourseDto>>
{
    protected override ApiMessage SuccessMessage => CourseMessageConsts.GetCoursesSuccess;

    protected override ApiMessage ErrorMessage => CourseMessageConsts.GetCoursesError;

    protected override async Task<List<CourseDto>> ExecuteAsync(GetUserCoursesQuery query, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();

        return await courseRepo.Find(c => c.Users.Any())
            .Where(c => c.Users.Any(u => u.Id == query.UserId))
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                LastSynched = c.LastSynched,
            })
            .ToListAsync(ct);
    }
}
