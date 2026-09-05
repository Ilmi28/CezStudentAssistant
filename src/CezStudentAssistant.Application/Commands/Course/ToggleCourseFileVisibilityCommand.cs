using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Course;

public class ToggleCourseFileVisibilityCommand : ICommand<CourseResourceDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid FileId { get; set; }
}

public class ToggleCourseFileVisibilityCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<ToggleCourseFileVisibilityCommand, CourseResourceDto>
{
    protected override string SuccessMessage => CourseMessageConsts.ToggleCourseFileVisibilitySuccess;
    protected override string ErrorMessage => CourseMessageConsts.ToggleCourseFileVisibilityError;

    protected override async Task<CourseResourceDto> ExecuteAsync(ToggleCourseFileVisibilityCommand command, CancellationToken ct)
    {
        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepository.GetByIdAsync(command.CourseId, ct, false, c => c.Users);

        if (course == null)
        {
            throw new NotFoundException(CourseMessageConsts.CourseNotFound);
        }

        if (!course.Users.Any(u => u.Id == command.UserId))
        {
            throw new UnauthorizedException(CourseMessageConsts.CourseAccessDenied);
        }

        var resourceRepository = unitOfWork.Repository<ICezResourceRepository>();
        var resource = await resourceRepository.GetByIdAsync(command.FileId, ct, false);

        if (resource == null || resource.CourseId != command.CourseId)
        {
            throw new NotFoundException(GeneralMessageConsts.FileNotFound);
        }

        resource.IsHidden = !resource.IsHidden;

        await resourceRepository.UpdateAsync(resource, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new CourseResourceDto
        {
            Id = resource.Id,
            DisplayName = resource.DisplayName,
            MimeType = resource.MimeType,
            LastModified = resource.LastModifiedAt,
            DownloadUrl = $"/course/{course.Id}/file/{resource.Id}/download",
            IsHidden = resource.IsHidden
        };
    }
}
