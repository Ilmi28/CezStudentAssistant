using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Course;

public class AddUserCourseCommand : ICommand<Guid>, IUserRequest
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class AddUserCourseCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<AddUserCourseCommand, Guid>
{
    protected override ApiMessage SuccessMessage => CourseMessageConsts.CreateCourseSuccess;
    protected override ApiMessage ErrorMessage => CourseMessageConsts.CreateCourseError;

    protected override async Task<Guid> ExecuteAsync(AddUserCourseCommand command, CancellationToken ct)
    {
        var userRepository = unitOfWork.Repository<IUserRepository>();
        var user = await userRepository.GetByIdAsync(command.UserId, ct);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var course = new Domain.Entities.Course
        {
            Name = command.Name,
            Description = command.Description,
            Type = CourseType.User,
            LastSynched = DateTime.UtcNow
        };

        course.Users.Add(user);

        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        await courseRepository.AddAsync(course, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return course.Id;
    }
}
