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

        using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonPayload = ExtractJsonObject(responseBody);

        using var jsonDocument = JsonDocument.Parse(jsonPayload);

        if (jsonDocument.RootElement.TryGetProperty("error", out _))
        {
            var error = JsonSerializer.Deserialize<ExternalCezErrorResponse>(jsonPayload)
                ?? throw new BadGatewayException(new ApiMessage(this, "CEZ returned an invalid error response."));

            if (string.Equals(error.ErrorCode, "missingparam", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(new ApiMessage(this, "Invalid request parameters."));

            if (string.Equals(error.ErrorCode, "invalidlogin", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedException(new ApiMessage(this, "Invalid username or password."));
            throw new BadGatewayException(new ApiMessage(this, $"CEZ returned an error: {error.Error}"));
        }

        var loginResponse = JsonSerializer.Deserialize<ExternalCezLoginResponse>(jsonPayload)
            ?? throw new BadGatewayException(new ApiMessage(this, "CEZ returned an invalid login response."));

        return new CezLoginResponse
        {
            Token = loginResponse.Token,
            PrivateToken = loginResponse.PrivateToken
        };
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
