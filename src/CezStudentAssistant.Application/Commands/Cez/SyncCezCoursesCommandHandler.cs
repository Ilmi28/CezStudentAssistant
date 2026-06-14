using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Application.Commands.Cez;

public sealed class SyncCezCoursesCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
}

public class SyncCezCoursesCommandHandler(ICezService cezService) : BaseCommandHandler<SyncCezCoursesCommand>
{
    protected override ApiMessage SuccessMessage => new(this, CezMessagesConsts.SyncCoursesSuccess);

    protected override ApiMessage ErrorMessage => new(this, CezMessagesConsts.GetUserCoursesError);

    protected override async Task ExecuteAsync(SyncCezCoursesCommand command, CancellationToken ct)
    {
        await cezService.SyncUserCourses(command.UserId, ct);
    }
}
