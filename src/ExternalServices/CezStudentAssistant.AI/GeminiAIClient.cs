using CezStudentAssistant.AI.Consts;
using CezStudentAssistant.AI.Services;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using CezStudentAssistant.Application.Responses.AI.Flashcard;

namespace CezStudentAssistant.AI;

public class GeminiAIClient(
    Client client,
    IConfiguration configuration,
    ILogger<GeminiAIClient> logger,
    IAIQuizService quizService,
    IAIFlashcardService flashcardService,
    IFileContentProcessorService fileContentProcessor) : IAIClient
{
    private readonly string _model = configuration["Gemini:DefaultModel"]
        ?? throw new InvalidOperationException("Configuration 'Gemini:DefaultModel' is missing or empty.");
    private readonly int _maxAttempts = int.TryParse(configuration["Gemini:MaxAttempts"], out var attempts) && attempts > 0
        ? attempts
        : throw new InvalidOperationException("Configuration 'Gemini:MaxAttempts' is missing or invalid.");
    private readonly int[] _retryDelaysMs = GetRetryDelays(configuration);

    public async Task<AIQuizResponse> GenerateQuizAsync(AIQuizRequest request)
    {
        var prompt = quizService.BuildPrompt(request);
        var schema = quizService.BuildSchema();

        var content = await BuildContentAsync(prompt, request.Files);

        var config = new GenerateContentConfig
        {
            ResponseMimeType = AIModelSettings.ResponseMimeTypeJson,
            ResponseSchema = schema
        };

        var response = await GenerateContentWithRetryAsync(content, config);

        var jsonText = response.Text;
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            logger.LogWarning("[GEMINI] AI returned an empty response text.");
            return new AIQuizResponse
            {
                Success = false,
                Message = AIErrorMessages.EmptyResponseErrorMessage
            };
        }

        var quizResponse = quizService.ParseResponse(jsonText);
        if (response.UsageMetadata?.TotalTokenCount.HasValue == true)
        {
            quizResponse.TotalTokens = response.UsageMetadata.TotalTokenCount.Value;
        }
        return quizResponse;
    }

    public async Task<AIFlashcardDeck> GenerateFlashcardsAsync(AIFlashcardRequest request)
    {
        var prompt = flashcardService.BuildPrompt(request);
        var schema = flashcardService.BuildSchema();

        var content = await BuildContentAsync(prompt, request.Files);

        var config = new GenerateContentConfig
        {
            ResponseMimeType = AIModelSettings.ResponseMimeTypeJson,
            ResponseSchema = schema
        };

        var response = await GenerateContentWithRetryAsync(content, config);

        var jsonText = response.Text;
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            logger.LogWarning("[GEMINI] AI returned an empty response text for flashcards.");
            throw new InvalidOperationException(AIErrorMessages.EmptyResponseErrorMessage);
        }

        var parsedDeck = flashcardService.ParseResponse(jsonText);
        if (parsedDeck == null || !parsedDeck.Cards.Any())
        {
            throw new InvalidOperationException("AI generated empty flashcards structure.");
        }

        return parsedDeck;
    }

    public async Task<int> EstimateTokenUsageAsync(AIQuizRequest request)
    {
        var prompt = quizService.BuildPrompt(request);
        var content = await BuildContentAsync(prompt, request.Files);

        var response = await client.Models.CountTokensAsync(_model, content);
        return response.TotalTokens ?? 0;
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
                if (ex.StatusCode != 429 && ex.StatusCode != 500 && ex.StatusCode != 503 && ex.StatusCode != 504)
                {
                    throw;
                }

                var delayIndex = Math.Min(attempts - 1, _retryDelaysMs.Length - 1);
                var delay = _retryDelaysMs[delayIndex];

                logger.LogWarning("[GEMINI] Attempt {Attempt} failed with status {StatusCode}: {Message}. Retrying in {Delay}ms...", attempts, ex.StatusCode, ex.Message, delay);
                await Task.Delay(delay);
            }
            catch (Exception ex) when (attempts < _maxAttempts)
            {
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
        if (delays.Count == 0)
        {
            throw new InvalidOperationException("Configuration 'Gemini:RetryDelaysMs' is missing or empty.");
        }
        return delays.ToArray();
    }

    private async Task<Content> BuildContentAsync(string prompt, IEnumerable<AIFile>? files)
    {
        var parts = new List<Part>
        {
            Part.FromText(prompt)
        };

        if (files != null)
        {
            foreach (var file in files)
            {
                var processed = await fileContentProcessor.ProcessFileAsync(file);
                if (processed != null)
                {
                    if (processed.IsTextFormat && processed.Text != null)
                    {
                        parts.Add(Part.FromText(processed.Text));
                    }
                    else if (processed.Bytes != null)
                    {
                        parts.Add(Part.FromBytes(processed.Bytes, processed.MimeType, null));
                    }
                }
            }
        }

        return new Content
        {
            Parts = parts
        };
    }
}
