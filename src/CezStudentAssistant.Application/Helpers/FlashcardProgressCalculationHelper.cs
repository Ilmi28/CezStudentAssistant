using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace CezStudentAssistant.Application.Helpers;

public static class FlashcardProgressCalculationHelper
{
    public static int GetCardDifficultyPoints(QuestionDifficulty difficulty) => difficulty switch
    {
        QuestionDifficulty.Easy => 1,
        QuestionDifficulty.Medium => 2,
        QuestionDifficulty.Hard => 3,
        _ => 2
    };

    public static bool IsCardMastered(Flashcard card, IEnumerable<FlashcardAttempt>? attempts)
    {
        if (card.State == FlashcardStateEnum.Mastered) return true;
        if (attempts == null) return false;
        return attempts.Any(a => a.Cards.Any(ac => ac.FlashcardId == card.Id && ac.State == FlashcardStateEnum.Mastered));
    }

    public static int CalculateProgressPercentage(IEnumerable<Flashcard> cards)
    {
        var cardList = cards.ToList();
        if (cardList.Count == 0) return 0;

        var totalMaxPoints = cardList.Sum(c => GetCardDifficultyPoints(c.Difficulty));
        if (totalMaxPoints == 0) return 0;

        var earnedPoints = cardList.Where(c => c.State == FlashcardStateEnum.Mastered).Sum(c => GetCardDifficultyPoints(c.Difficulty));
        var percentage = ((double)earnedPoints / totalMaxPoints) * 100;

        return (int)System.Math.Min(100, System.Math.Round(percentage));
    }

    public static int CalculateDeckProgressPercentage(IEnumerable<Flashcard> cards, IEnumerable<FlashcardAttempt>? attempts)
    {
        var cardList = cards.ToList();
        if (cardList.Count == 0) return 0;

        var attemptList = attempts?.ToList() ?? new List<FlashcardAttempt>();
        var totalMaxPoints = cardList.Sum(c => GetCardDifficultyPoints(c.Difficulty));
        if (totalMaxPoints == 0) return 0;

        var earnedPoints = cardList
            .Where(c => IsCardMastered(c, attemptList))
            .Sum(c => GetCardDifficultyPoints(c.Difficulty));

        var percentage = ((double)earnedPoints / totalMaxPoints) * 100;
        return (int)System.Math.Min(100, System.Math.Round(percentage));
    }

    public static int DetermineAttemptCardCount(int commandCardCount, int? deckCardCountPerAttempt, int totalDeckCards)
    {
        if (commandCardCount > 0)
        {
            return commandCardCount;
        }

        if (deckCardCountPerAttempt.HasValue && deckCardCountPerAttempt.Value > 0)
        {
            return deckCardCountPerAttempt.Value;
        }

        return totalDeckCards;
    }

    public static int CalculateAttemptProgressPercentage(IEnumerable<(QuestionDifficulty Difficulty, FlashcardStateEnum State)> cards)
    {
        var cardList = cards.ToList();
        if (cardList.Count == 0) return 0;

        var totalMaxPoints = cardList.Sum(c => GetCardDifficultyPoints(c.Difficulty));
        if (totalMaxPoints == 0) return 0;

        var earnedPoints = cardList.Where(c => c.State == FlashcardStateEnum.Mastered).Sum(c => GetCardDifficultyPoints(c.Difficulty));
        var percentage = ((double)earnedPoints / totalMaxPoints) * 100;

        return (int)System.Math.Min(100, System.Math.Round(percentage));
    }

    public static int CalculateAttemptProgressPercentage(int masteredCount, int learningCount, int totalCardCount)
    {
        var total = totalCardCount > 0 ? totalCardCount : 1;
        var percentage = ((double)masteredCount / total) * 100;
        return (int)System.Math.Min(100, System.Math.Round(percentage));
    }
}
