using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Course;

public class DeleteUserCourseCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
}

public class DeleteUserCourseCommandHandler(IUnitOfWork unitOfWork, IFileService fileService) : BaseCommandHandler<DeleteUserCourseCommand>
{
    protected override ApiMessage SuccessMessage => CourseMessageConsts.DeleteCourseSuccess;
    protected override ApiMessage ErrorMessage => CourseMessageConsts.DeleteCourseError;

    protected override async Task ExecuteAsync(DeleteUserCourseCommand command, CancellationToken ct)
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

        await courseRepository.DeleteAsync(course, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
