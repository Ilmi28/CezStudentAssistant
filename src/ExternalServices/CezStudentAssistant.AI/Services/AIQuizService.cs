using AutoMapper;
using CezStudentAssistant.AI.Consts.Quiz;
using CezStudentAssistant.AI.Responses.Quiz;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CezStudentAssistant.AI.Services;

public class AIQuizService(IMapper mapper, ILogger<AIQuizService> logger) : IAIQuizService
{
    public string BuildPrompt(AIQuizRequest request)
    {
        var languageName = request.Language switch
        {
            QuizLanguage.PL => "Polish",
            QuizLanguage.EN => "English",
            _ => "Polish"
        };

        var template = request.GenerateFromPromptOnly
            ? QuizPrompts.InstructionPromptFromPromptOnlyTemplate
            : QuizPrompts.InstructionPromptTemplate;

        var instructionPrompt = string.Format(template, request.QuestionCount, languageName);

        if (request.EasyCount.HasValue || request.MediumCount.HasValue || request.HardCount.HasValue)
        {
            var easy = request.EasyCount ?? 0;
            var medium = request.MediumCount ?? 0;
            var hard = request.HardCount ?? 0;
            instructionPrompt += string.Format(QuizPrompts.DifficultyBreakdownTemplate, easy, medium, hard, request.QuestionCount);
        }

        if (!string.IsNullOrWhiteSpace(request.AdditionalInstructions))
        {
            instructionPrompt += string.Format(QuizPrompts.AdditionalInstructionsTemplate, request.AdditionalInstructions);
        }

        return instructionPrompt;
    }

    public Schema BuildSchema()
    {
        var optionSchema = new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { QuizSchemas.PropertyContent, QuizSchemas.PropertyIsCorrect },
            Properties = new Dictionary<string, Schema>
            {
                { QuizSchemas.PropertyContent, new Schema { Type = Google.GenAI.Types.Type.String, Description = QuizSchemas.OptionContentDescription } },
                { QuizSchemas.PropertyIsCorrect, new Schema { Type = Google.GenAI.Types.Type.Boolean, Description = QuizSchemas.OptionIsCorrectDescription } }
            }
        };

        var allowedQuestionTypes = Enum.GetNames<QuestionType>().ToList();
        var allowedDifficulties = Enum.GetNames<QuestionDifficulty>().ToList();

        var questionSchema = new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { QuizSchemas.PropertyContent, QuizSchemas.PropertyQuestionType, QuizSchemas.PropertyDifficulty, QuizSchemas.PropertyOptions },
            Properties = new Dictionary<string, Schema>
            {
                { QuizSchemas.PropertyContent, new Schema { Type = Google.GenAI.Types.Type.String, Description = QuizSchemas.QuestionContentDescription } },
                { QuizSchemas.PropertyQuestionType, new Schema { 
                    Type = Google.GenAI.Types.Type.String, 
                    Description = string.Format(QuizSchemas.QuestionTypeDescriptionTemplate, string.Join(", ", allowedQuestionTypes)),
                    Enum = allowedQuestionTypes
                } },
                { QuizSchemas.PropertyDifficulty, new Schema { 
                    Type = Google.GenAI.Types.Type.String, 
                    Description = string.Format(QuizSchemas.QuestionDifficultyDescriptionTemplate, string.Join(", ", allowedDifficulties)),
                    Enum = allowedDifficulties
                } },
                { QuizSchemas.PropertyOptions, new Schema { 
                    Type = Google.GenAI.Types.Type.Array, 
                    Items = optionSchema, 
                    Description = QuizSchemas.QuestionOptionsDescription 
                } }
            }
        };

        return new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { QuizSchemas.PropertyTitle, QuizSchemas.PropertyDescription, QuizSchemas.PropertyQuestions },
            Properties = new Dictionary<string, Schema>
            {
                { QuizSchemas.PropertyTitle, new Schema { Type = Google.GenAI.Types.Type.String, Description = QuizSchemas.QuizTitleDescription } },
                { QuizSchemas.PropertyDescription, new Schema { Type = Google.GenAI.Types.Type.String, Description = QuizSchemas.QuizDescriptionDescription } },
                { QuizSchemas.PropertyQuestions, new Schema { 
                    Type = Google.GenAI.Types.Type.Array, 
                    Items = questionSchema, 
                    Description = QuizSchemas.QuizQuestionsDescription 
                } }
            }
        };
    }

    public AIQuizResponse ParseResponse(string jsonText)
    {
        var externalQuiz = JsonSerializer.Deserialize<ExternalAIQuiz>(jsonText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (externalQuiz == null)
        {
            logger.LogWarning("[AI] Failed to deserialize the JSON returned by AI: {JsonText}", jsonText);
            return new AIQuizResponse
            {
                Success = false,
                Message = "Failed to deserialize the quiz structure returned by AI."
            };
        }

        var quiz = mapper.Map<AIQuiz>(externalQuiz);

        return new AIQuizResponse
        {
            Success = true,
            Data = quiz
        };
    }
}
