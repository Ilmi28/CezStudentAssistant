using CezStudentAssistant.AI.Consts;
using CezStudentAssistant.AI.Consts.Chat;
using CezStudentAssistant.AI.Services;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

using CezStudentAssistant.Application.Responses.AI.Flashcard;

using CezStudentAssistant.Domain.Enums;

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

        var content = await BuildContentAsync(prompt, request.Files, request.AdditionalInstructions);

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

        var content = await BuildContentAsync(prompt, request.Files, request.AdditionalInstructions);

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
        var content = await BuildContentAsync(prompt, request.Files, request.AdditionalInstructions);

        var response = await client.Models.CountTokensAsync(_model, content);
        return response.TotalTokens ?? 0;
    }

    public async Task<int> EstimateTextTokenUsageAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var content = new Content
        {
            Parts = [new Part { Text = text }]
        };

        var response = await client.Models.CountTokensAsync(_model, content);
        return response.TotalTokens ?? 0;
    }

    public async IAsyncEnumerable<string> StreamChatResponseAsync(
        IEnumerable<CezStudentAssistant.Domain.Entities.ChatMessage> history,
        string userPrompt,
        IEnumerable<AIFile>? files = null,
        string? courseName = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var contents = new List<Content>();

        foreach (var msg in history)
        {
            var role = msg.Role == ChatMessageRole.Assistant ? "model" : "user";

            contents.Add(new Content
            {
                Role = role,
                Parts = [new Part { Text = msg.Content }]
            });
        }

        var userParts = new List<Part>
        {
            Part.FromText(userPrompt)
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
                        userParts.Add(Part.FromText(processed.Text));
                    }
                    else if (processed.Bytes != null)
                    {
                        userParts.Add(Part.FromBytes(processed.Bytes, processed.MimeType, null));
                    }
                }
            }
        }

        contents.Add(new Content
        {
            Role = "user",
            Parts = userParts
        });

        var systemInstructionText = !string.IsNullOrWhiteSpace(courseName)
            ? $"{ChatPrompts.DefaultSystemInstruction}\n\n[CONTEXT - UNIVERSITY COURSE]:\nYou are assisting the student specifically with the university course: \"{courseName}\". Tailor your explanations, terminology, examples, and context to this course."
            : ChatPrompts.DefaultSystemInstruction;

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content
            {
                Parts = [new Part { Text = systemInstructionText }]
            }
        };

        var attempts = 0;
        var hasYielded = false;
        Exception? caughtException = null;

        while (true)
        {
            attempts++;
            IAsyncEnumerator<GenerateContentResponse>? enumerator = null;

            try
            {
                var stream = client.Models.GenerateContentStreamAsync(_model, contents, config);
                enumerator = stream.GetAsyncEnumerator(ct);
            }
            catch (ClientError ex) when (!hasYielded && attempts < _maxAttempts)
            {
                if (ex.StatusCode != 429 && ex.StatusCode != 500 && ex.StatusCode != 503 && ex.StatusCode != 504)
                {
                    caughtException = ex;
                    break;
                }

                var delayIndex = Math.Min(attempts - 1, _retryDelaysMs.Length - 1);
                var delay = _retryDelaysMs[delayIndex];
                logger.LogWarning("[GEMINI] Chat stream attempt {Attempt} failed with status {StatusCode}: {Message}. Retrying in {Delay}ms...", attempts, ex.StatusCode, ex.Message, delay);
                await Task.Delay(delay, ct);
                continue;
            }
            catch (Exception ex) when (!hasYielded && attempts < _maxAttempts)
            {
                var delayIndex = Math.Min(attempts - 1, _retryDelaysMs.Length - 1);
                var delay = _retryDelaysMs[delayIndex];
                logger.LogWarning(ex, "[GEMINI] Chat stream attempt {Attempt} failed: {Message}. Retrying in {Delay}ms...", attempts, ex.Message, delay);
                await Task.Delay(delay, ct);
                continue;
            }
            catch (Exception ex)
            {
                caughtException = ex;
                break;
            }

            var retryRequested = false;

            try
            {
                while (true)
                {
                    bool hasNext;
                    try
                    {
                        hasNext = await enumerator.MoveNextAsync();
                    }
                    catch (ClientError ex) when (!hasYielded && attempts < _maxAttempts)
                    {
                        if (ex.StatusCode != 429 && ex.StatusCode != 500 && ex.StatusCode != 503 && ex.StatusCode != 504)
                        {
                            caughtException = ex;
                            break;
                        }

                        var delayIndex = Math.Min(attempts - 1, _retryDelaysMs.Length - 1);
                        var delay = _retryDelaysMs[delayIndex];
                        logger.LogWarning("[GEMINI] Chat stream MoveNextAsync attempt {Attempt} failed with status {StatusCode}: {Message}. Retrying in {Delay}ms...", attempts, ex.StatusCode, ex.Message, delay);
                        await Task.Delay(delay, ct);
                        retryRequested = true;
                        break;
                    }
                    catch (Exception ex) when (!hasYielded && attempts < _maxAttempts)
                    {
                        var delayIndex = Math.Min(attempts - 1, _retryDelaysMs.Length - 1);
                        var delay = _retryDelaysMs[delayIndex];
                        logger.LogWarning(ex, "[GEMINI] Chat stream MoveNextAsync attempt {Attempt} failed: {Message}. Retrying in {Delay}ms...", attempts, ex.Message, delay);
                        await Task.Delay(delay, ct);
                        retryRequested = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                        break;
                    }

                    if (!hasNext)
                    {
                        yield break;
                    }

                    var responseChunk = enumerator.Current;
                    if (!string.IsNullOrEmpty(responseChunk.Text))
                    {
                        hasYielded = true;
                        yield return responseChunk.Text;
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    await enumerator.DisposeAsync();
                }
            }

            if (retryRequested)
            {
                continue;
            }

            break;
        }

        if (caughtException != null)
        {
            logger.LogError(caughtException, "[GEMINI] Chat stream encountered unrecoverable error.");
            var fallback = hasYielded
                ? "\n\n*Coś poszło nie tak. Spróbuj ponownie później.*"
                : "*Coś poszło nie tak. Spróbuj ponownie później.*";
            yield return fallback;
        }
    }

    public async Task<string> GenerateChatTitleAsync(string userMessage, string assistantResponse, CancellationToken ct = default)
    {
        var prompt = string.Format(ChatPrompts.GenerateTitlePrompt, userMessage, assistantResponse);
        try
        {
            var content = Part.FromText(prompt);
            var response = await client.Models.GenerateContentAsync(_model, new Content { Parts = [content] });
            var rawTitle = response.Text?.Trim();

            if (string.IsNullOrWhiteSpace(rawTitle))
            {
                return string.Empty;
            }

            var cleanTitle = rawTitle.Trim('"', '\'', '`', ' ', '\n', '\r');
            if (cleanTitle.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
            {
                cleanTitle = cleanTitle["Title:".Length..].Trim();
            }

            return cleanTitle.Length > 50 ? cleanTitle[..50] + "..." : cleanTitle;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[GEMINI] Failed to generate AI chat title.");
            return string.Empty;
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

    private async Task<Content> BuildContentAsync(string prompt, IEnumerable<AIFile>? files, string? additionalInstructions = null)
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

        if (!string.IsNullOrWhiteSpace(additionalInstructions))
        {
            var hasFiles = files != null && files.Any();
            var groundingNote = hasFiles
                ? "Ensure all generated content remains factually grounded in the provided document(s)."
                : "Ensure all generated content is factually accurate and directly addresses the user's requested focus topic using expert domain knowledge.";

            parts.Add(Part.FromText($"""
                [FINAL DIRECTIVE - USER CUSTOM FOCUS]:
                REMINDER: Focus your output questions/cards, title, and description specifically according to the user's requested focus:
                "{additionalInstructions}"
                {groundingNote}
                """));
        }

        return new Content
        {
            Parts = parts
        };
    }
}
