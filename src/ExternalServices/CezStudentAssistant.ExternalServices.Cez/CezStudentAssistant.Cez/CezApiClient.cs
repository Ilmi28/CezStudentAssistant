using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez.Consts;
using CezStudentAssistant.Cez.Requests;
using CezStudentAssistant.Cez.Responses;
using System.Text.Json;

namespace CezStudentAssistant.Cez;

internal class CezApiClient(HttpClient httpClient) : ICezApiClient
{

    public async Task<CezLoginResponse> LoginToCez(CezLoginRequest loginDto)
    {
        var externalRequest = new ExternalCezLoginRequest
        {
            Username = loginDto.UserName,
            Password = loginDto.Password
        };

        var requestUri =
            $"{CezBaseConsts.LoginPath}?username={Uri.EscapeDataString(externalRequest.Username)}" +
            $"&password={Uri.EscapeDataString(externalRequest.Password)}" +
            $"&service={CezBaseConsts.Service}";

        try
        {
            using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonPayload = ExtractJsonObject(responseBody);

            using var jsonDocument = JsonDocument.Parse(jsonPayload);

            if (jsonDocument.RootElement.TryGetProperty("error", out _))
            {
                var error = JsonSerializer.Deserialize<ExternalCezLoginErrorResponse>(jsonPayload);

                return new CezLoginResponse
                {
                    Success = false,
                    Message = error?.Error,
                    ErrorCode = error?.ErrorCode,
                    Data = null
                };
            }

            var loginResponse = JsonSerializer.Deserialize<ExternalCezLoginResponse>(jsonPayload);

            return new CezLoginResponse
            {
                Success = true,
                Message = null,
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
                Message = "An error occurred while processing the CEZ login response.",
                ErrorCode = null,
                Data = null
            };
        }
    }

    public Task<CezGetUserCoursesResponse> GetUserCourses(CezUserRequest request)
    {
        throw new NotImplementedException();
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
