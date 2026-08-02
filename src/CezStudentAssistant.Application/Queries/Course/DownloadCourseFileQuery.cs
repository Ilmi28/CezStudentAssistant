using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using MediatR;

namespace CezStudentAssistant.Application.Queries.Course;

public class DownloadCourseFileQuery : IRequest<FileResultDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid FileId { get; set; }
}

public class DownloadCourseFileQueryHandler(IUnitOfWork unitOfWork, IFileService fileService) : IRequestHandler<DownloadCourseFileQuery, FileResultDto>
{
    public async Task<FileResultDto> Handle(DownloadCourseFileQuery request, CancellationToken cancellationToken)
    {
        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepository.GetByIdAsync(request.CourseId, cancellationToken, true, c => c.Users);

        if (course == null)
        {
            throw new NotFoundException(CourseMessageConsts.CourseNotFound);
        }

        if (!course.Users.Any(u => u.Id == request.UserId))
        {
            throw new UnauthorizedException(CourseMessageConsts.CourseAccessDenied);
        }

        var resourceRepository = unitOfWork.Repository<ICezResourceRepository>();
        var resource = await resourceRepository.GetByIdAsync(request.FileId, cancellationToken, true);

        if (resource == null || resource.CourseId != request.CourseId)
        {
            throw new NotFoundException(GeneralMessageConsts.FileNotFound);
        }

        var filePath = $"{course.Id}/{resource.Name}";
        var stream = await fileService.DownloadAsync(filePath, ContainerNameConsts.CourseFilesContainer, cancellationToken);

        return new FileResultDto
        {
            FileStream = stream,
            ContentType = resource.MimeType,
            FileName = resource.DisplayName
        };
    }
}
