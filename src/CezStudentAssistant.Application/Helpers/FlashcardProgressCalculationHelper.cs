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
}
