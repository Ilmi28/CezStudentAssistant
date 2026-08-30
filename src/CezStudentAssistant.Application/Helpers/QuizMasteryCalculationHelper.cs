using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CezStudentAssistant.Application.Helpers;

public static class QuizMasteryCalculationHelper
{
    public record QuizMasteryResult(int MasteredCount, int TotalPoolCount, int ProgressPercentage);

    public static QuizMasteryResult CalculateMastery(
        IEnumerable<Question> questions,
        IEnumerable<QuizAttempt> attempts)
    {
        var questionList = questions?.ToList() ?? new List<Question>();
        var completedAttempts = attempts?.Where(a => a.Status == QuizAttemptStatus.Completed).ToList() ?? new List<QuizAttempt>();

        int masteredCount = 0;
        int totalPoolCount = questionList.Count;

        int getPoints(QuestionDifficulty diff) => diff switch
        {
            QuestionDifficulty.Easy => 1,
            QuestionDifficulty.Medium => 2,
            QuestionDifficulty.Hard => 3,
            _ => 2
        };

        var totalMaxPoints = questionList.Sum(q => getPoints(q.Difficulty));
        int earnedMasteredPoints = 0;

        if (totalPoolCount > 0 && completedAttempts.Count > 0)
        {
            foreach (var question in questionList)
            {
                var correctOptionIds = question.Options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Id)
                    .ToHashSet();

                if (correctOptionIds.Count == 0) continue;

                bool isAnsweredCorrectly = completedAttempts.Any(a =>
                {
                    var ans = a.Answers.FirstOrDefault(ansItem => ansItem.QuestionId == question.Id);
                    if (ans == null) return false;

                    var selected = ans.SelectedOptions
                        .Select(so => so.QuestionOptionId)
                        .ToHashSet();

                    if (question.Type == QuestionType.SingleChoice)
                    {
                        return selected.Count == 1 && correctOptionIds.Contains(selected.First());
                    }

                    return selected.SetEquals(correctOptionIds);
                });

                if (isAnsweredCorrectly)
                {
                    masteredCount++;
                    earnedMasteredPoints += getPoints(question.Difficulty);
                }
            }
        }

        int percentage = totalMaxPoints > 0
            ? (int)Math.Round((double)earnedMasteredPoints / totalMaxPoints * 100)
            : 0;

        return new QuizMasteryResult(masteredCount, totalPoolCount, percentage);
    }
}
