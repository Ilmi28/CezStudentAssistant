using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez.Consts;
using CezStudentAssistant.Cez.Requests;
using CezStudentAssistant.Cez.Responses;
using System.Globalization;
using System.Text.Json;

namespace CezStudentAssistant.Cez;

internal class CezApiClient(HttpClient httpClient) : ICezApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<CezLoginResponse> LoginToCez(CezLoginRequest loginDto)
    {
        var externalRequest = new ExternalCezLoginRequest
        {
            Username = loginDto.UserName,
            Password = loginDto.Password
        };

        var requestUri = BuildRequestUri(
            CezBaseConsts.LoginPath,
            [
                new("username", externalRequest.Username),
                new("password", externalRequest.Password),
                new("service", CezBaseConsts.Service)
            ]
        );

        var requestResult = await SendGetAsync<ExternalCezLoginResponse>(requestUri);
        if (requestResult.Error is not null)
        {
            return new CezLoginResponse
            {
                Success = false,
                Message = requestResult.Error.Message ?? requestResult.Error.Error,
                ErrorCode = requestResult.Error.ErrorCode,
                Data = null
            };
        }

        return new CezLoginResponse
        {
            Success = requestResult.Data is not null,
            Message = null,
            ErrorCode = null,
            Data = requestResult.Data is null
                ? null
                : new CezTokens
                {
                    Token = requestResult.Data.Token,
                    PrivateToken = requestResult.Data.PrivateToken
                }
        };
    }

    public async Task<CezGetUserCoursesResponse> GetUserCourses(CezUserRequest request)
    {
        var externalRequest = new ExternalCezUserRequest
        {
            Token = request.Token,
            Function = CezFunctionConsts.GetUserCourses,
            UserId = request.UserId
        };

        var requestUri = BuildRequestUri(
            CezBaseConsts.FunctionsPath,
            [
                new("wstoken", externalRequest.Token),
                new("wsfunction", externalRequest.Function),
                new("moodlewsrestformat", externalRequest.RestFormat),
                new("userid", externalRequest.UserId)
            ]
        );

        var requestResult = await SendGetAsync<List<ExternalCezGetUserCoursesResponse>>(requestUri);
        if (requestResult.Error is not null)
        {
            return new CezGetUserCoursesResponse
            {
                Success = false,
                Message = requestResult.Error.Message ?? requestResult.Error.Error,
                ErrorCode = requestResult.Error.ErrorCode
            };
        }

        var firstCourse = requestResult.Data?.FirstOrDefault();

        return new CezGetUserCoursesResponse
        {
            Success = firstCourse is not null,
            Message = null,
            ErrorCode = null,
            ExternalId = firstCourse?.Id.ToString(CultureInfo.InvariantCulture),
            ShortName = firstCourse?.ShortName,
            FullName = firstCourse?.FullName,
            DisplayName = firstCourse?.DisplayName,
            CourseImage = firstCourse?.CourseImage
        };
    }

    private async Task<CezRequestResult<TData>> SendGetAsync<TData>(string requestUri)
        where TData : class
    {
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

    private sealed record CezRequestResult<TData>(TData? Data, ExternalCezErrorResponse? Error)
        where TData : class;
}
