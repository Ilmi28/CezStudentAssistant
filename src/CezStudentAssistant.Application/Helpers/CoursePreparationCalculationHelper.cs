using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CezStudentAssistant.Application.Helpers;

public static class CoursePreparationCalculationHelper
{
    public record CoursePreparationResult(
        int? PreparationPercentage,
        int? QuizProgressPercentage,
        int? FlashcardProgressPercentage
    );

    public static CoursePreparationResult CalculatePreparation(
        IEnumerable<Quiz> quizzes,
        IEnumerable<FlashcardDeck> decks)
    {
        var quizList = quizzes?.ToList() ?? new List<Quiz>();
        var deckList = decks?.ToList() ?? new List<FlashcardDeck>();

        // 1. Quizzes Progress Calculation (Difficulty-weighted: Easy=1, Medium=2, Hard=3)
        int? quizProgressPercentage = null;
        var quizzesWithAttempts = quizList
            .Where(q => q.Questions.Count > 0 && q.Attempts.Any(a => a.Status == QuizAttemptStatus.Completed))
            .ToList();

        if (quizzesWithAttempts.Count > 0)
        {
            var quizPercentages = quizzesWithAttempts.Select(q =>
            {
                var mastery = QuizMasteryCalculationHelper.CalculateMastery(q.Questions, q.Attempts);
                return mastery.ProgressPercentage;
            }).ToList();

            quizProgressPercentage = (int)Math.Round(quizPercentages.Average());
        }

        // 2. Flashcards Progress Calculation (Difficulty-weighted: Easy=1, Medium=2, Hard=3)
        int? flashcardProgressPercentage = null;
        var allCards = deckList.SelectMany(d => d.Cards).ToList();
        var allAttempts = deckList.SelectMany(d => d.Attempts).ToList();

        if (allCards.Count > 0)
        {
            flashcardProgressPercentage = FlashcardProgressCalculationHelper.CalculateDeckProgressPercentage(allCards, allAttempts);
        }

        // 3. Overall Preparation Percentage
        int? preparationPercentage = null;
        if (quizProgressPercentage.HasValue && flashcardProgressPercentage.HasValue)
        {
            preparationPercentage = (int)Math.Round((quizProgressPercentage.Value + flashcardProgressPercentage.Value) / 2.0);
        }
        else if (quizProgressPercentage.HasValue)
        {
            preparationPercentage = quizProgressPercentage.Value;
        }
        else if (flashcardProgressPercentage.HasValue)
        {
            preparationPercentage = flashcardProgressPercentage.Value;
        }

        return new CoursePreparationResult(
            preparationPercentage,
            quizProgressPercentage,
            flashcardProgressPercentage
        );
    }
}
