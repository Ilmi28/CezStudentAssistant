using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Exceptions;
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
    protected override ApiMessage SuccessMessage => new(this, CourseMessageConsts.GetCourseDetailsSuccess);
    protected override ApiMessage ErrorMessage => new(this, CourseMessageConsts.GetCourseDetailsError);

    protected override async Task<CourseDetailsDto> ExecuteAsync(GetCourseDetailsQuery query, CancellationToken ct)
    {
        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepository.GetByIdAsync(query.CourseId, ct, true, c => c.Users);

        if (course == null)
        {
            throw new NotFoundException(new ApiMessage(this, CourseMessageConsts.CourseNotFound));
        }

        if (!course.Users.Any(u => u.Id == query.UserId))
        {
            throw new UnauthorizedException(new ApiMessage(this, CourseMessageConsts.CourseAccessDenied));
        }

        var resourceRepository = unitOfWork.Repository<ICezResourceRepository>();
        var resources = await resourceRepository.Find(r => r.CourseId == query.CourseId, true).ToListAsync(ct);

        var dto = new CourseDetailsDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Type = course.Type,
            LastSynched = course.LastSynched,
            Files = resources.Select(r => new CourseResourceDto
            {
                Id = r.Id,
                DisplayName = r.DisplayName,
                MimeType = r.MimeType,
                LastModified = r.LastModifiedAt,
                DownloadUrl = $"/course/{course.Id}/file/{r.Id}/download"
            }).ToList()
        };

        return dto;
    }
}
