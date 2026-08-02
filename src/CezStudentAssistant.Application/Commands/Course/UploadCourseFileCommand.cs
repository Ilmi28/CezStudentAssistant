using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Course;

public class UploadCourseFileCommand : ICommand<Guid>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public required Stream FileStream { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
}

public class UploadCourseFileCommandHandler(IUnitOfWork unitOfWork, IFileService fileService) : BaseCommandHandler<UploadCourseFileCommand, Guid>
{
    protected override ApiMessage SuccessMessage => CourseMessageConsts.UploadCourseFileSuccess;
    protected override ApiMessage ErrorMessage => CourseMessageConsts.UploadCourseFileError;

    protected override async Task<Guid> ExecuteAsync(UploadCourseFileCommand command, CancellationToken ct)
    {
        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepository.GetByIdAsync(command.CourseId, ct, false, c => c.Users);

        if (course == null)
        {
            throw new NotFoundException(CourseMessageConsts.CourseNotFound);
        }

        if (course.Type == CourseType.Cez)
        {
            throw new BadRequestException(CourseMessageConsts.CourseInvalidType);
        }

        if (!course.Users.Any(u => u.Id == command.UserId))
        {
            throw new UnauthorizedException(CourseMessageConsts.CourseAccessDenied);
        }

        var resourceRepository = unitOfWork.Repository<ICezResourceRepository>();

        var contentName = $"{Guid.NewGuid()}_{command.FileName}";
        var filePath = $"{course.Id}/{contentName}";

        await fileService.UploadAsync(command.FileStream, filePath, ContainerNameConsts.CourseFilesContainer, command.ContentType, ct);

        var resource = new Resource
        {
            CourseId = course.Id,
            Name = contentName,
            DisplayName = command.FileName,
            MimeType = command.ContentType,
            CezLastModified = DateTime.UtcNow
        };

        await resourceRepository.AddAsync(resource, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return resource.Id;
    }
}
