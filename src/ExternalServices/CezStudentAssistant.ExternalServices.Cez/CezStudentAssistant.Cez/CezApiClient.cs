using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez.Consts;
using CezStudentAssistant.Cez.Requests;
using CezStudentAssistant.Cez.Responses;
using System.Globalization;

namespace CezStudentAssistant.Cez;

internal class CezApiClient(ICezRequestExecutor requestExecutor) : ICezApiClient
{
    public async Task<CezLoginResponse> LoginToCez(CezLoginRequest loginDto)
    {
        var externalRequest = new ExternalCezLoginRequest
        {
            Username = loginDto.UserName,
            Password = loginDto.Password
        };

        return await ExecuteRequestAsync<ExternalCezLoginResponse, CezLoginResponse>(
            CezBaseConsts.LoginPath,
            [
                new(CezParamsConsts.Username, externalRequest.Username),
                new(CezParamsConsts.Password, externalRequest.Password),
                new(CezParamsConsts.Service, CezBaseConsts.Service)
            ],
            error => new CezLoginResponse
            {
                Success = false,
                Message = error.ErrorCode switch
                {
                    CezErrorConsts.MissingParam => CezResponseMessageConsts.MissingParam,
                    CezErrorConsts.InvalidLogin => CezResponseMessageConsts.InvalidLogin,
                    _ => CezResponseMessageConsts.UnexpectedError
                },
                ErrorCode = error.ErrorCode,
                Data = null
            },
            data => new CezLoginResponse
            {
                Success = data is not null,
                Message = null,
                ErrorCode = null,
                Data = data is null
                    ? null
                    : new CezTokens
                    {
                        Token = data.Token,
                        PrivateToken = data.PrivateToken
                    }
            }
        );
    }

    public async Task<CezGetUserCoursesResponse> GetUserCourses(CezUserRequest request)
    {
        var externalRequest = new ExternalCezUserRequest
        {
            Token = request.Token,
            Function = CezFunctionConsts.GetUserCourses,
            UserId = request.UserId
        };

        return await ExecuteRequestAsync<List<ExternalCezGetUserCoursesResponse>, CezGetUserCoursesResponse>(
            CezBaseConsts.FunctionsPath,
            [
                new(CezParamsConsts.Token, externalRequest.Token),
                new(CezParamsConsts.Function, externalRequest.Function),
                new(CezParamsConsts.RestFormat, externalRequest.RestFormat),
                new(CezParamsConsts.UserId, externalRequest.UserId)
            ],
            error => new CezGetUserCoursesResponse
            {
                Success = false,
                Message = error.Message ?? error.Error,
                ErrorCode = error.ErrorCode
            },
            data =>
            {
                var courses = data?.Select(course => new CezCourse
                {
                    ExternalId = course.Id.ToString(CultureInfo.InvariantCulture),
                    ShortName = course.ShortName,
                    FullName = course.FullName,
                    DisplayName = course.DisplayName,
                    CourseImage = course.CourseImage
                }).ToList();

                return new CezGetUserCoursesResponse
                {
                    Success = courses is { Count: > 0 },
                    Message = null,
                    ErrorCode = null,
                    Data = courses
                };
            }
        );
    }

    public async Task<CezGetSiteInfoResponse> GetSiteInfo(CezBaseRequest request)
    {
        var externalRequest = new ExternalCezBaseRequest
        {
            Token = request.Token,
            Function = CezFunctionConsts.GetSiteInfo,
        };
        return await ExecuteRequestAsync<ExternalGetSiteInfoResponse, CezGetSiteInfoResponse>(
            CezBaseConsts.FunctionsPath,
            [
                new(CezParamsConsts.Token, externalRequest.Token),
                new(CezParamsConsts.Function, externalRequest.Function),
                new(CezParamsConsts.RestFormat, externalRequest.RestFormat),
            ],
            error => new CezGetSiteInfoResponse
            {
                Success = false,
                Message = error.Message ?? error.Error,
                ErrorCode = error.ErrorCode
            },
            data => new CezGetSiteInfoResponse
            {
                Success = data is not null,
                Message = null,
                ErrorCode = null,
                Data = data is null
                    ? null
                    : new CezSiteInfo
                    {
                        UserName = data.UserName,
                        FullName = data.FullName,
                        ExternalUserId = data.UserId
                    }
            }
        );
    }

    private async Task<TResult> ExecuteRequestAsync<TData, TResult>(
        string path,
        IEnumerable<KeyValuePair<string, string>> queryParams,
        Func<ExternalCezErrorResponse, TResult> mapError,
        Func<TData?, TResult> mapSuccess
    )
        where TData : class
    {
        var requestResult = await requestExecutor.SendGetAsync<TData>(path, queryParams);

        return requestResult.Error is not null
            ? mapError(requestResult.Error)
            : mapSuccess(requestResult.Data);
    }
}
