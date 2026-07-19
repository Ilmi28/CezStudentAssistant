using AutoMapper;
using CezStudentAssistant.AI.Consts;
using CezStudentAssistant.AI.Responses.Quiz;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CezStudentAssistant.AI;

public class GeminiAIClient(Client client, IMapper mapper, IConfiguration configuration, ILogger<GeminiAIClient> logger) : IAIClient
{
    private readonly string _model = configuration["Gemini:DefaultModel"] ?? GeminiAIModelSettings.DefaultModel;
    private readonly int _maxAttempts = int.TryParse(configuration["Gemini:MaxAttempts"], out var attempts) && attempts > 0 ? attempts : 6;
    private readonly int[] _retryDelaysMs = GetRetryDelays(configuration);

    public async Task<AIQuizResponse> GenerateQuizAsync(AIQuizRequest request)
    {
        try
        {
            var content = await BuildQuizContentAsync(request);
            var quizSchema = BuildQuizSchema();

            var config = new GenerateContentConfig
            {
                ResponseMimeType = GeminiAIModelSettings.ResponseMimeTypeJson,
                ResponseSchema = quizSchema
            };

            var response = await GenerateContentWithRetryAsync(content, config);

            var jsonText = response.Text;
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                logger.LogWarning("[GEMINI] AI returned an empty response text.");
                return new AIQuizResponse
                {
                    Success = false,
                    Message = GeminiAIErrorMessages.EmptyResponseErrorMessage
                };
            }

            var externalQuiz = JsonSerializer.Deserialize<ExternalAIQuiz>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (externalQuiz == null)
            {
                logger.LogWarning("[GEMINI] Failed to deserialize the JSON returned by AI: {JsonText}", jsonText);
                return new AIQuizResponse
                {
                    Success = false,
                    Message = GeminiAIErrorMessages.DeserializationErrorMessage
                };
            }

            var quiz = mapper.Map<AIQuiz>(externalQuiz);

            return new AIQuizResponse
            {
                Success = true,
                Data = quiz
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[GEMINI] Failed to generate quiz: {Message}", ex.Message);
            return new AIQuizResponse
            {
                Success = false,
                Message = string.Format(GeminiAIErrorMessages.GeneralErrorMessageFormat, ex.Message)
            };
        }
    }

    private async Task<GenerateContentResponse> GenerateContentWithRetryAsync(Content content, GenerateContentConfig config)
    {
        var attempts = 0;

        while (true)
        {
            try
            {
                attempts++;
                return await client.Models.GenerateContentAsync(_model, content, config);
            }
            catch (ClientError ex) when (attempts < _maxAttempts)
            {
                // Only retry on transient status codes (429 Rate Limit, 500 Server Error, 503 Service Unavailable, 504 Gateway Timeout)
                if (ex.StatusCode != 429 && ex.StatusCode != 500 && ex.StatusCode != 503 && ex.StatusCode != 504)
                {
                    throw;
                }

                // Protect against out of bounds index if delays list is smaller than max attempts
                var delayIndex = Math.Min(attempts - 1, _retryDelaysMs.Length - 1);
                var delay = _retryDelaysMs[delayIndex];

                logger.LogWarning("[GEMINI] Attempt {Attempt} failed with status {StatusCode}: {Message}. Retrying in {Delay}ms...", attempts, ex.StatusCode, ex.Message, delay);
                await Task.Delay(delay);
            }
            catch (Exception ex) when (attempts < _maxAttempts)
            {
                // Catch other network/HTTP exceptions
                var delayIndex = Math.Min(attempts - 1, _retryDelaysMs.Length - 1);
                var delay = _retryDelaysMs[delayIndex];

                logger.LogWarning(ex, "[GEMINI] Attempt {Attempt} failed: {Message}. Retrying in {Delay}ms...", attempts, ex.Message, delay);
                await Task.Delay(delay);
            }
        }
    }

    private static int[] GetRetryDelays(IConfiguration configuration)
    {
        var delays = new List<int>();
        var section = configuration.GetSection("Gemini:RetryDelaysMs");
        foreach (var child in section.GetChildren())
        {
            if (int.TryParse(child.Value, out var delay))
            {
                delays.Add(delay);
            }
        }
        return delays.Count > 0 ? delays.ToArray() : new[] { 1000, 2000, 4000, 8000, 16000, 32000 };
    }

    private static async Task<Content> BuildQuizContentAsync(AIQuizRequest request)
    {
        var parts = new List<Part>();

        var languageName = request.Language switch
        {
            QuizLanguage.PL => "Polish",
            QuizLanguage.EN => "English",
            _ => "Polish"
        };

        var instructionPrompt = string.Format(GeminiAIPrompts.InstructionPromptTemplate, request.QuestionCount, languageName);

        if (!string.IsNullOrWhiteSpace(request.AdditionalInstructions))
        {
            instructionPrompt += string.Format(GeminiAIPrompts.AdditionalInstructionsTemplate, request.AdditionalInstructions);
        }

        parts.Add(Part.FromText(instructionPrompt));

        if (request.Files != null)
        {
            foreach (var file in request.Files)
            {
                using var ms = new MemoryStream();
                await file.Stream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                if (bytes.Length > 0)
                {
                    parts.Add(Part.FromBytes(bytes, file.MimeType, null));
                }
            }
        }

        return new Content
        {
            Parts = parts
        };
    }

    private static Schema BuildQuizSchema()
    {
        var optionSchema = new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { GeminiAISchemas.PropertyContent, GeminiAISchemas.PropertyIsCorrect },
            Properties = new Dictionary<string, Schema>
            {
                { GeminiAISchemas.PropertyContent, new Schema { Type = Google.GenAI.Types.Type.String, Description = GeminiAISchemas.OptionContentDescription } },
                { GeminiAISchemas.PropertyIsCorrect, new Schema { Type = Google.GenAI.Types.Type.Boolean, Description = GeminiAISchemas.OptionIsCorrectDescription } }
            }
        };

        var allowedQuestionTypes = Enum.GetNames<QuestionType>().ToList();

        var questionSchema = new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { GeminiAISchemas.PropertyContent, GeminiAISchemas.PropertyQuestionType, GeminiAISchemas.PropertyPoints, GeminiAISchemas.PropertyOptions },
            Properties = new Dictionary<string, Schema>
            {
                { GeminiAISchemas.PropertyContent, new Schema { Type = Google.GenAI.Types.Type.String, Description = GeminiAISchemas.QuestionContentDescription } },
                { GeminiAISchemas.PropertyQuestionType, new Schema {
                    Type = Google.GenAI.Types.Type.String,
                    Description = string.Format(GeminiAISchemas.QuestionTypeDescriptionTemplate, string.Join(", ", allowedQuestionTypes)),
                    Enum = allowedQuestionTypes
                } },
                { GeminiAISchemas.PropertyPoints, new Schema { Type = Google.GenAI.Types.Type.Number, Description = GeminiAISchemas.QuestionPointsDescription } },
                { GeminiAISchemas.PropertyOptions, new Schema {
                    Type = Google.GenAI.Types.Type.Array,
                    Items = optionSchema,
                    Description = GeminiAISchemas.QuestionOptionsDescription
                } }
            }
        };

        return new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Required = new List<string> { GeminiAISchemas.PropertyTitle, GeminiAISchemas.PropertyDescription, GeminiAISchemas.PropertyQuestions },
            Properties = new Dictionary<string, Schema>
            {
                { GeminiAISchemas.PropertyTitle, new Schema { Type = Google.GenAI.Types.Type.String, Description = GeminiAISchemas.QuizTitleDescription } },
                { GeminiAISchemas.PropertyDescription, new Schema { Type = Google.GenAI.Types.Type.String, Description = GeminiAISchemas.QuizDescriptionDescription } },
                { GeminiAISchemas.PropertyQuestions, new Schema {
                    Type = Google.GenAI.Types.Type.Array,
                    Items = questionSchema,
                    Description = GeminiAISchemas.QuizQuestionsDescription
                } }
            }
        };
    }
}
