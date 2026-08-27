using CezStudentAssistant.AI.Consts.Flashcard;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Flashcard;
using CezStudentAssistant.Domain.Enums;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using System.Text.Json.Serialization;

namespace CezStudentAssistant.AI.Services;

public class AIFlashcardService(ILogger<AIFlashcardService> logger) : IAIFlashcardService
{
    public string BuildPrompt(AIFlashcardRequest request)
    {
        var languageName = request.Language switch
        {
            QuizLanguage.PL => "Polish",
            QuizLanguage.EN => "English",
            _ => "Polish"
        };

        var prompt = string.Format(FlashcardPrompts.InstructionPromptTemplate, request.CardCount, languageName);

        if (request.EasyCount.HasValue || request.MediumCount.HasValue || request.HardCount.HasValue)
        {
            var easy = request.EasyCount ?? 0;
            var medium = request.MediumCount ?? 0;
            var hard = request.HardCount ?? 0;
            prompt += string.Format(FlashcardPrompts.DifficultyBreakdownTemplate, easy, medium, hard, request.CardCount);
        }

        if (!string.IsNullOrWhiteSpace(request.AdditionalInstructions))
        {
            prompt += string.Format(FlashcardPrompts.AdditionalInstructionsTemplate, request.AdditionalInstructions);
        }

        return prompt;
    }

    public Schema BuildSchema()
    {
        var allowedDifficulties = Enum.GetNames<QuestionDifficulty>().ToList();

        var cardSchema = new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { FlashcardSchemas.PropertyFront, FlashcardSchemas.PropertyBack, FlashcardSchemas.PropertyDifficulty },
            Properties = new Dictionary<string, Schema>
            {
                { FlashcardSchemas.PropertyFront, new Schema { Type = Google.GenAI.Types.Type.String, Description = FlashcardSchemas.CardFrontDescription } },
                { FlashcardSchemas.PropertyBack, new Schema { Type = Google.GenAI.Types.Type.String, Description = FlashcardSchemas.CardBackDescription } },
                { FlashcardSchemas.PropertyDifficulty, new Schema {
                    Type = Google.GenAI.Types.Type.String,
                    Description = string.Format(FlashcardSchemas.CardDifficultyDescriptionTemplate, string.Join(", ", allowedDifficulties)),
                    Enum = allowedDifficulties
                } }
            }
        };

        return new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { FlashcardSchemas.PropertyTitle, FlashcardSchemas.PropertyDescription, FlashcardSchemas.PropertyCards },
            Properties = new Dictionary<string, Schema>
            {
                { FlashcardSchemas.PropertyTitle, new Schema { Type = Google.GenAI.Types.Type.String, Description = FlashcardSchemas.DeckTitleDescription } },
                { FlashcardSchemas.PropertyDescription, new Schema { Type = Google.GenAI.Types.Type.String, Description = FlashcardSchemas.DeckDescriptionDescription } },
                { FlashcardSchemas.PropertyCards, new Schema {
                    Type = Google.GenAI.Types.Type.Array,
                    Items = cardSchema,
                    Description = FlashcardSchemas.DeckCardsDescription
                } }
            }
        };
    }

    public AIFlashcardDeck ParseResponse(string jsonText)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var deck = JsonSerializer.Deserialize<AIFlashcardDeck>(jsonText, options);

            return deck ?? new AIFlashcardDeck
            {
                Title = "Zestaw fiszek",
                Description = "Wygenerowano fiszki z materiałów",
                Cards = new List<AIFlashcard>()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[AI] ParseResponse for flashcards failed: {Message}. Raw JSON: {JsonText}", ex.Message, jsonText);
            return new AIFlashcardDeck
            {
                Title = "Zestaw fiszek",
                Description = "Wygenerowano fiszki z materiałów",
                Cards = new List<AIFlashcard>()
            };
        }
    }
}
