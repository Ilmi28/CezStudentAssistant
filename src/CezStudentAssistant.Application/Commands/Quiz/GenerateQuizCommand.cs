using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class GenerateQuizCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int QuestionCount { get; set; }
    public string? AdditionalInstructions { get; set; }
}

public class GenerateQuizCommandHandler(
    IJobScheduler jobScheduler,
    IJobService jobService) : BaseCommandHandler<GenerateQuizCommand>
{
    protected override ApiMessage SuccessMessage => new(this, AIMessageConsts.QuizGenerationEnqueued);

    protected override ApiMessage ErrorMessage => new(this, AIMessageConsts.QuizGenerationError);

    protected override async Task ExecuteAsync(GenerateQuizCommand command, CancellationToken ct)
    {
        var job = await jobService.CreateJobAsync(command.UserId, JobType.QuizGeneration, ct);

        var dto = new GenerateQuizDto
        {
            UserId = command.UserId,
            CourseId = command.CourseId,
            QuestionCount = command.QuestionCount,
            Language = QuizLanguage.PL,
            AdditionalInstructions = command.AdditionalInstructions
        };

        var jobId = jobScheduler.Enqueue<IAIService>(service => service.GenerateQuiz(dto, ct));

        await jobService.UpdateJobAsync(job, JobStatus.Enqueued, jobId, ct);
    }
}
