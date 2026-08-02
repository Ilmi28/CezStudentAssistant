using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Cez;

public sealed class SyncCezCoursesCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
}

public class SyncCezCoursesCommandHandler(
    IJobScheduler jobScheduler,
    IJobService jobService) : BaseCommandHandler<SyncCezCoursesCommand>
{
    protected override string SuccessMessage => CezMessagesConsts.SyncCoursesSuccess;

    protected override string ErrorMessage => CezMessagesConsts.GetUserCoursesError;

    protected override async Task ExecuteAsync(SyncCezCoursesCommand command, CancellationToken ct)
    {
        var job = await jobService.CreateJobAsync(command.UserId, JobType.CezSync, ct);

        var jobId = jobScheduler.Enqueue<ICezService>(service => service.SyncUserCourses(command.UserId, ct));

        await jobService.UpdateJobAsync(job, JobStatus.Enqueued, jobId, ct);
    }
}
