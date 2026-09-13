using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Queries.Course;

public sealed class GetUserCoursesQuery : BasePagedQuery<CourseDto>
{
}

public class GetUserCoursesQueryHandler(IUnitOfWork unitOfWork)
    : BasePagedQueryHandler<GetUserCoursesQuery, Domain.Entities.Course, CourseDto>
{
    protected override string SuccessMessage => CourseMessageConsts.GetCoursesSuccess;

    protected override string ErrorMessage => CourseMessageConsts.GetCoursesError;

    protected override Task<IQueryable<Domain.Entities.Course>> GetQueryableAsync(GetUserCoursesQuery query, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();

        var queryable = courseRepo.Find(c => c.Users.Any(u => u.Id == query.UserId));

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var rawTerm = query.SearchTerm.Trim().ToLower();
            var normalizedTerm = TextNormalizationHelper.Normalize(rawTerm);

            queryable = queryable.Where(c =>
                c.Name.ToLower().Contains(rawTerm) ||
                c.Name.ToLower().Contains(normalizedTerm));
        }

        queryable = queryable.OrderBy(c => c.Name);

        return Task.FromResult(queryable);
    }

    protected override CourseDto MapToDto(Domain.Entities.Course c, GetUserCoursesQuery query)
    {
        return new CourseDto
        {
            Id = c.Id,
            Name = c.Name,
            LastSynched = c.LastSynched,
            IsCez = c.CezExternalId != null,
        };
    }
}
