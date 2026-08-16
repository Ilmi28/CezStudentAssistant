using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Application.Helpers;

public static class QuizPointHelper
{
    public static decimal CalculatePoints(QuestionDifficulty difficulty) => difficulty switch
    {
        QuestionDifficulty.Easy => 1m,
        QuestionDifficulty.Medium => 2m,
        QuestionDifficulty.Hard => 3m,
        _ => 1m
    };
}
