using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez.Responses;
using System.Text.Json;

namespace CezStudentAssistant.Cez;

internal class CezApiClient(HttpClient httpClient) : ICezApiClient
{
    private const string LoginPath = "login/token.php";
    private const string Service = "moodle_mobile_app";

    public async Task<CezLoginResponse> LoginToCez(CezLoginRequest loginDto)
    {
        var requestUri =
            $"{LoginPath}?username={Uri.EscapeDataString(loginDto.UserName)}" +
            $"&password={Uri.EscapeDataString(loginDto.Password)}" +
            $"&service={Service}";

        try
        {
            using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonPayload = ExtractJsonObject(responseBody);

            using var jsonDocument = JsonDocument.Parse(jsonPayload);

            if (jsonDocument.RootElement.TryGetProperty("error", out _))
            {
                var error = JsonSerializer.Deserialize<ExternalCezErrorResponse>(jsonPayload);

                return new CezLoginResponse
                {
                    Success = false,
                    Error = error?.Error,
                    ErrorCode = error?.ErrorCode,
                    Data = null
                };
            }

            var loginResponse = JsonSerializer.Deserialize<ExternalCezLoginResponse>(jsonPayload);

            return new CezLoginResponse
            {
                Success = true,
                Error = null,
                ErrorCode = null,
                Data = new CezTokens
                {
                    Token = loginResponse?.Token ?? string.Empty,
                    PrivateToken = loginResponse?.PrivateToken ?? string.Empty
                }
            };
        }
        catch (Exception)
        {
            return new CezLoginResponse
            {
                Success = false,
                Error = "An error occurred while processing the CEZ login response.",
                ErrorCode = null,
                Data = null
            };
        }
    }

    private static string ExtractJsonObject(string responseBody)
    {
        var start = responseBody.IndexOf('{');
        var end = responseBody.LastIndexOf('}');

        if (start < 0 || end < start)
            throw new BadGatewayException(new ApiMessage(typeof(CezApiClient), "CEZ response did not contain valid JSON."));

        return responseBody[start..(end + 1)];
    }
}
