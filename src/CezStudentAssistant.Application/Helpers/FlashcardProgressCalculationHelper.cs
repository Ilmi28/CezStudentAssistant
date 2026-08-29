using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace CezStudentAssistant.Application.Helpers;

public static class FlashcardProgressCalculationHelper
{
    public static int CalculateProgressPercentage(IEnumerable<Flashcard> cards)
    {
        var cardList = cards.ToList();
        if (cardList.Count == 0) return 0;

        var masteredCount = cardList.Count(c => c.State == FlashcardStateEnum.Mastered);
        var percentage = ((double)masteredCount / cardList.Count) * 100;

        return (int)System.Math.Round(percentage);
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

    public static int CalculateAttemptProgressPercentage(int masteredCount, int learningCount, int totalCardCount)
    {
        var total = totalCardCount > 0 ? totalCardCount : 1;
        var percentage = ((masteredCount * 1.0 + learningCount * 0.5) / total) * 100;
        return (int)System.Math.Min(100, System.Math.Round(percentage));
    }
}
