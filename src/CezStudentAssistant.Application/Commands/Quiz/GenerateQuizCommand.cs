using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class GenerateQuizCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public string? AdditionalInstructions { get; set; }
}

public class GenerateQuizCommandHandler(
    IJobScheduler jobScheduler,
    IJobService jobService,
    IUnitOfWork unitOfWork) : BaseCommandHandler<GenerateQuizCommand>
{
    protected override string SuccessMessage => AIMessageConsts.QuizGenerationEnqueued;

    protected override string ErrorMessage => AIMessageConsts.QuizGenerationError;

    protected override async Task ExecuteAsync(GenerateQuizCommand command, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetByIdAsync(command.CourseId, ct);
        if (course == null)
            throw new NotFoundException(AIMessageConsts.CourseNotFound);

        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var existingQuizzes = quizRepo.Find(q => q.CourseId == command.CourseId).ToList();
        var quizTitle = $"Quiz #{existingQuizzes.Count + 1}";

        var quiz = new Domain.Entities.Quiz
        {
            UserId = command.UserId,
            Name = quizTitle,
            DisplayName = quizTitle,
            CourseId = command.CourseId,
            Status = QuizStatusEnum.Generating,
            TimeLimitMinutes = command.TimeLimitMinutes
        };

        await quizRepo.AddAsync(quiz, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var job = await jobService.CreateJobAsync(command.UserId, JobType.QuizGeneration, ct);

        var dto = new GenerateQuizDto
        {
            QuizId = quiz.Id,
            UserId = command.UserId,
            CourseId = command.CourseId,
            QuestionCount = command.QuestionCount,
            TimeLimitMinutes = command.TimeLimitMinutes,
            Language = QuizLanguage.PL,
            AdditionalInstructions = command.AdditionalInstructions
        };

        var jobId = jobScheduler.Enqueue<IQuizGenerationService>(service => service.GenerateQuiz(dto, ct));
        await jobService.UpdateJobAsync(job, JobStatus.Enqueued, jobId, ct);
    }
}
