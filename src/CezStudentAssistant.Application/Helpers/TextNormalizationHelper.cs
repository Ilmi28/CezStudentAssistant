using System;
using System.Globalization;
using System.Text;

namespace CezStudentAssistant.Application.Helpers;

public static class TextNormalizationHelper
{
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalizedString = text
            .Replace("ł", "l")
            .Replace("Ł", "L")
            .Normalize(NormalizationForm.FormD);

        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }
}
