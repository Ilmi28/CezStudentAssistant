using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace CezStudentAssistant.Application.Commands.Course;

public class DeleteCourseFileCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid FileId { get; set; }
}

public class DeleteCourseFileCommandHandler(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IConfiguration configuration) : BaseCommandHandler<DeleteCourseFileCommand>
{
    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? throw new InvalidOperationException(CourseMessageConsts.CourseFilesContainerConfigMissing);

    protected override string SuccessMessage => CourseMessageConsts.DeleteCourseFileSuccess;
    protected override string ErrorMessage => CourseMessageConsts.DeleteCourseFileError;

    protected override async Task ExecuteAsync(DeleteCourseFileCommand command, CancellationToken ct)
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
        var resource = await resourceRepository.GetByIdAsync(command.FileId, ct, false);

        if (resource == null || resource.CourseId != command.CourseId)
        {
            throw new NotFoundException(GeneralMessageConsts.FileNotFound);
        }

        var filePath = $"{course.Id}/{resource.Name}";
        await fileService.DeleteAsync(filePath, _containerName, ct);

        await resourceRepository.DeleteAsync(resource, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
