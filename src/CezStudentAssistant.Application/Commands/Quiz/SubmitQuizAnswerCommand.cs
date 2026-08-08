using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class SubmitQuizAnswerCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizAttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public List<Guid> QuestionOptionIds { get; set; } = new List<Guid>();
}

public class SubmitQuizAnswerCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<SubmitQuizAnswerCommand>
{
    protected override string SuccessMessage => QuizMessageConsts.AnswerSubmittedSuccess;
    protected override string ErrorMessage => QuizMessageConsts.AnswerSubmittedError;

    protected async override Task ExecuteAsync(SubmitQuizAnswerCommand command, CancellationToken ct)
    {
        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var questionRepo = unitOfWork.Repository<IQuestionRepository>();
        var questionAnswerRepo = unitOfWork.Repository<IQuestionAnswerRepository>();
        var selectedQuizOptionRepo = unitOfWork.Repository<ISelectedQuizOptionRepository>();

        var quizAttempt = await quizAttemptRepo.GetByIdAsync(command.QuizAttemptId, ct)
            ?? throw new NotFoundException(QuizMessageConsts.QuizAttemptNotFound);

        ValidateQuizAttempt(command, quizAttempt);

        var question = await questionRepo.GetByIdAsync(command.QuestionId, ct, false, x => x.Options)
            ?? throw new NotFoundException(QuizMessageConsts.QuestionNotFound);

        await ValidateQuestionAndOptionsAsync(command, quizAttempt, question, questionAnswerRepo, ct);

        var distinctOptionIds = command.QuestionOptionIds.Distinct().ToList();

        var questionAnswer = new QuestionAnswer
        {
            QuizAttemptId = command.QuizAttemptId,
            QuestionId = command.QuestionId,
            EarnedPoints = null
        };
        await questionAnswerRepo.AddAsync(questionAnswer, ct);

        var selectedQuizOptions = distinctOptionIds.Select(optionId => new SelectedQuizOption
        {
            QuestionOptionId = optionId,
            QuestionAnswerId = questionAnswer.Id
        }).ToList();

        await selectedQuizOptionRepo.AddRangeAsync(selectedQuizOptions, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private void ValidateQuizAttempt(SubmitQuizAnswerCommand command, QuizAttempt quizAttempt)
    {
        if (quizAttempt.UserId != command.UserId)
            throw new ForbiddenException(QuizMessageConsts.UnauthorizedAttemptAccess);

        if (quizAttempt.Status != QuizAttemptStatus.InProgress)
            throw new BadRequestException(QuizMessageConsts.QuizAttemptNotInProgress);

        if (quizAttempt.ExpiresAt.HasValue && quizAttempt.ExpiresAt.Value < DateTime.UtcNow)
            throw new BadRequestException(QuizMessageConsts.QuizAttemptExpired);
    }

    private async Task ValidateQuestionAndOptionsAsync(
        SubmitQuizAnswerCommand command,
        QuizAttempt quizAttempt,
        Question question,
        IQuestionAnswerRepository questionAnswerRepo,
        CancellationToken ct)
    {
        if (question.QuizId != quizAttempt.QuizId)
            throw new BadRequestException(QuizMessageConsts.QuestionNotBelongToQuiz);

        var alreadyAnswered = await questionAnswerRepo.ExistsAsync(x => x.QuizAttemptId == command.QuizAttemptId && x.QuestionId == command.QuestionId, ct);
        if (alreadyAnswered)
            throw new BadRequestException(QuizMessageConsts.QuestionAlreadyAnswered);

        var distinctOptionIds = command.QuestionOptionIds.Distinct().ToList();
        if (question.Type == QuestionType.SingleChoice && distinctOptionIds.Count > 1)
            throw new BadRequestException(QuizMessageConsts.SingleChoiceMultipleOptions);

        var questionOptions = question.Options;
        foreach (var optionId in distinctOptionIds)
        {
            if (!questionOptions.Any(x => x.Id == optionId))
                throw new BadRequestException(QuizMessageConsts.QuestionOptionNotFound);
        }
    }
}
