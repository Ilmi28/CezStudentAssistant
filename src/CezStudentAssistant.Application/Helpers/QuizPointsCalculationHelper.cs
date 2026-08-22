using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using System.Linq;

namespace CezStudentAssistant.Application.Helpers;

public static class QuizPointsCalculationHelper
{
    private static decimal CalculatePoints(QuestionDifficulty difficulty) => difficulty switch
    {
        QuestionDifficulty.Easy => 1m,
        QuestionDifficulty.Medium => 2m,
        QuestionDifficulty.Hard => 3m,
        _ => 1m
    };

    public static decimal CalculatePoints(QuizAttempt attempt)
    {
        if (attempt.Quiz == null || attempt.Answers == null)
            return 0m;

        decimal totalPoints = 0m;
        foreach (var answer in attempt.Answers)
        {
            var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question != null)
            {
                totalPoints += CalculateQuestionPoints(question, answer);
            }
        }

        return Math.Round(totalPoints, 2);
    }

    private static decimal CalculateQuestionPoints(Question question, QuestionAnswer answer)
    {
        var maxPoints = CalculatePoints(question.Difficulty);
        var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
        if (correctOptionIds.Count == 0)
            return 0m;

        var selectedOptionIds = answer.SelectedOptions.Select(so => so.QuestionOptionId).ToHashSet();

        if (question.Type == QuestionType.SingleChoice)
        {
            var isSingleCorrect = selectedOptionIds.Count == 1 && correctOptionIds.Contains(selectedOptionIds.First());
            return isSingleCorrect ? maxPoints : 0m;
        }

        var correctSelectedCount = selectedOptionIds.Count(id => correctOptionIds.Contains(id));
        var incorrectSelectedCount = selectedOptionIds.Count(id => !correctOptionIds.Contains(id));

        var netCorrect = (decimal)(correctSelectedCount - incorrectSelectedCount);
        if (netCorrect <= 0m)
            return 0m;

        var fraction = netCorrect / correctOptionIds.Count;
        return Math.Round(maxPoints * fraction, 2);
    }
}
