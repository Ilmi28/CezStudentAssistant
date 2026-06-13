using CezStudentAssistant.Cez.Interfaces;
using CezStudentAssistant.Cez.Responses;
using System.Text.Json;

namespace CezStudentAssistant.Cez.Services;

internal class CezRequestService(HttpClient httpClient) : ICezRequestService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<CezRequestResult<TData>> SendGetAsync<TData>(
        string path,
        IEnumerable<KeyValuePair<string, string>> queryParams
    )
        where TData : class
    {
        var requestUri = BuildRequestUri(path, queryParams);

        using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonPayload = ExtractJsonPayload(responseBody);

        var error = TryGetError(jsonPayload);
        if (error is not null)
        {
            return new CezRequestResult<TData>(null, error);
        }

        var data = JsonSerializer.Deserialize<TData>(jsonPayload, JsonOptions);
        return new CezRequestResult<TData>(data, null);
    }

    private static ExternalCezErrorResponse? TryGetError(string jsonPayload)
    {
        using var jsonDocument = JsonDocument.Parse(jsonPayload);
        var root = jsonDocument.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (
            root.TryGetProperty("error", out _) ||
            root.TryGetProperty("exception", out _) ||
            root.TryGetProperty("errorcode", out _)
        )
        {
            return JsonSerializer.Deserialize<ExternalCezErrorResponse>(jsonPayload, JsonOptions);
        }

        return null;
    }

    private static string BuildRequestUri(string path, IEnumerable<KeyValuePair<string, string>> queryParams)
    {
        var query = string.Join(
            "&",
            queryParams.Select(param => $"{param.Key}={Uri.EscapeDataString(param.Value)}")
        );

        return $"{path}?{query}";
    }

    private static string ExtractJsonPayload(string responseBody)
    {
        var objectStart = responseBody.IndexOf('{');
        var arrayStart = responseBody.IndexOf('[');

        var isArray = arrayStart >= 0 && (objectStart < 0 || arrayStart < objectStart);
        var start = isArray ? arrayStart : objectStart;
        var end = isArray ? responseBody.LastIndexOf(']') : responseBody.LastIndexOf('}');

        return start < 0 || end < start ? responseBody : responseBody[start..(end + 1)];
    }
}

internal sealed record CezRequestResult<TData>(TData? Data, ExternalCezErrorResponse? Error)
    where TData : class;