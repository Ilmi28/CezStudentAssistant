using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class StartQuizCommand : ICommand<QuizAttemptDetailsDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
}

public class StartQuizCommandHandler(IUnitOfWork unitOfWork) : BaseCommandHandler<StartQuizCommand, QuizAttemptDetailsDto>
{
    protected override string SuccessMessage => QuizMessageConsts.StartQuizSuccess;

    protected override string ErrorMessage => QuizMessageConsts.StartQuizError;

    protected override async Task<QuizAttemptDetailsDto> ExecuteAsync(StartQuizCommand command, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var quiz = await quizRepo.Find(q => q.Id == command.QuizId && (q.UserId == command.UserId || q.Course.Users.Any(u => u.Id == command.UserId)))
            .Include(q => q.Course)
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(ct);

        if (quiz == null)
            throw new NotFoundException(QuizMessageConsts.QuizNotFound);

        List<Question> selectedQuestions;

        bool hasSpecificDifficultyConfig = (quiz.EasyQuestionCountPerAttempt.HasValue && quiz.EasyQuestionCountPerAttempt.Value > 0) ||
                                           (quiz.MediumQuestionCountPerAttempt.HasValue && quiz.MediumQuestionCountPerAttempt.Value > 0) ||
                                           (quiz.HardQuestionCountPerAttempt.HasValue && quiz.HardQuestionCountPerAttempt.Value > 0);

        if (hasSpecificDifficultyConfig)
        {
            var easyPool = quiz.Questions.Where(q => q.Difficulty == QuestionDifficulty.Easy).OrderBy(_ => Random.Shared.Next()).ToList();
            var mediumPool = quiz.Questions.Where(q => q.Difficulty == QuestionDifficulty.Medium).OrderBy(_ => Random.Shared.Next()).ToList();
            var hardPool = quiz.Questions.Where(q => q.Difficulty == QuestionDifficulty.Hard).OrderBy(_ => Random.Shared.Next()).ToList();

            int takeEasy = Math.Min(quiz.EasyQuestionCountPerAttempt ?? 0, easyPool.Count);
            int takeMedium = Math.Min(quiz.MediumQuestionCountPerAttempt ?? 0, mediumPool.Count);
            int takeHard = Math.Min(quiz.HardQuestionCountPerAttempt ?? 0, hardPool.Count);

            selectedQuestions = new List<Question>();
            selectedQuestions.AddRange(easyPool.Take(takeEasy));
            selectedQuestions.AddRange(mediumPool.Take(takeMedium));
            selectedQuestions.AddRange(hardPool.Take(takeHard));

            selectedQuestions = selectedQuestions
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            if (selectedQuestions.Count == 0)
            {
                int questionsToTake = quiz.QuestionCountPerAttempt.HasValue && quiz.QuestionCountPerAttempt.Value > 0
                    ? Math.Min(quiz.QuestionCountPerAttempt.Value, quiz.Questions.Count)
                    : quiz.Questions.Count;

                selectedQuestions = quiz.Questions
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(questionsToTake)
                    .ToList();
            }
        }
        else
        {
            int questionsToTake = quiz.QuestionCountPerAttempt.HasValue && quiz.QuestionCountPerAttempt.Value > 0
                ? Math.Min(quiz.QuestionCountPerAttempt.Value, quiz.Questions.Count)
                : quiz.Questions.Count;

            selectedQuestions = quiz.Questions
                .OrderBy(_ => Random.Shared.Next())
                .Take(questionsToTake)
                .ToList();
        }

        decimal maxPoints = selectedQuestions.Sum(q => q.Difficulty == QuestionDifficulty.Easy ? 1m : q.Difficulty == QuestionDifficulty.Hard ? 3m : 2m);

        var quizAttemptRepo = unitOfWork.Repository<IQuizAttemptRepository>();
        var attempt = new QuizAttempt
        {
            QuizId = quiz.Id,
            Course = quiz.Course,
            UserId = command.UserId,
            Status = QuizAttemptStatus.InProgress,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            QuestionCount = selectedQuestions.Count,
            MaxPoints = maxPoints,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = quiz.TimeLimitMinutes.HasValue ? DateTime.UtcNow.AddMinutes(quiz.TimeLimitMinutes.Value) : null
        };

        foreach (var q in selectedQuestions)
        {
            attempt.Answers.Add(new QuestionAnswer
            {
                QuestionId = q.Id
            });
        }

        await quizAttemptRepo.AddAsync(attempt, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new QuizAttemptDetailsDto
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            DisplayName = quiz.DisplayName,
            CourseName = quiz.Course.Name,
            Status = attempt.Status,
            IsPending = true,
            TimeLimitMinutes = attempt.TimeLimitMinutes,
            QuestionCount = attempt.QuestionCount,
            MaxPoints = attempt.MaxPoints,
            StartedAt = attempt.StartedAt,
            ExpiresAt = attempt.ExpiresAt,
            Questions = selectedQuestions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Content = q.Content,
                Type = q.Type,
                Difficulty = q.Difficulty,
                Options = q.Options.OrderBy(_ => Random.Shared.Next()).Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList(),
            Answers = attempt.Answers.Select(ans => new QuestionAnswerDto
            {
                Id = ans.Id,
                QuestionId = ans.QuestionId,
                SelectedOptionIds = ans.SelectedOptions.Select(so => so.QuestionOptionId).ToList()
            }).ToList()
        };
    }
}
