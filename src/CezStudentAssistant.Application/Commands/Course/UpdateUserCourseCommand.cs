using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Course;

public class UpdateUserCourseCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateUserCourseCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<UpdateUserCourseCommand>
{
    protected override ApiMessage SuccessMessage => new(this, CourseMessageConsts.UpdateCourseSuccess);
    protected override ApiMessage ErrorMessage => new(this, CourseMessageConsts.UpdateCourseError);

    protected override async Task ExecuteAsync(UpdateUserCourseCommand command, CancellationToken ct)
    {
        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepository.GetByIdAsync(command.CourseId, ct, false, c => c.Users);

        if (course == null)
        {
            throw new NotFoundException(new ApiMessage(this, CourseMessageConsts.CourseNotFound));
        }

        if (course.Type == CourseType.Cez)
        {
            throw new BadRequestException(new ApiMessage(this, CourseMessageConsts.CourseInvalidType));
        }

        if (!course.Users.Any(u => u.Id == command.UserId))
        {
            throw new UnauthorizedException(new ApiMessage(this, CourseMessageConsts.CourseAccessDenied));
        }

        course.Name = command.Name;
        course.Description = command.Description;

        await courseRepository.UpdateAsync(course, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
