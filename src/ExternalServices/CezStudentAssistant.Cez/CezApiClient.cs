using AutoMapper;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez.Consts;
using CezStudentAssistant.Cez.Interfaces;
using CezStudentAssistant.Cez.Requests;
using CezStudentAssistant.Cez.Responses;

namespace CezStudentAssistant.Cez;

internal class CezApiClient(ICezRequestService requestService, IMapper mapper) : ICezApiClient
{
    public async Task<CezLoginResponse> LoginToCez(CezLoginRequest loginDto)
    {
        var externalRequest = new ExternalCezLoginRequest
        {
            Username = loginDto.UserName,
            Password = loginDto.Password
        };

        var requestResult = await requestService.SendGetAsync<ExternalCezLoginResponse>(
            CezBaseConsts.LoginPath,
            [
                new(CezParamsConsts.Username, externalRequest.Username),
                new(CezParamsConsts.Password, externalRequest.Password),
                new(CezParamsConsts.Service, CezBaseConsts.Service)
            ]
        );

        return mapper.Map<CezLoginResponse>(requestResult);
    }

    public async Task<CezGetUserCoursesResponse> GetUserCourses(CezUserRequest request)
    {
        var externalRequest = new ExternalCezUserRequest
        {
            Token = request.Token,
            Function = CezFunctionConsts.GetUserCourses,
            UserId = request.UserId
        };

        var requestResult = await requestService.SendGetAsync<List<ExternalCezGetUserCoursesResponse>>(
            CezBaseConsts.FunctionsPath,
            [
                new(CezParamsConsts.Token, externalRequest.Token),
                new(CezParamsConsts.Function, externalRequest.Function),
                new(CezParamsConsts.RestFormat, externalRequest.RestFormat),
                new(CezParamsConsts.UserId, externalRequest.UserId.ToString())
            ]
        );

        return mapper.Map<CezGetUserCoursesResponse>(requestResult);
    }

    public async Task<CezGetUserResponse> GetUser(CezGetUserRequest request)
    {
        var requestResult = await requestService.SendGetAsync<List<ExternalCezGetUserResponse>>(
            CezBaseConsts.FunctionsPath,
            [
                new(CezParamsConsts.Token, request.Token),
                new(CezParamsConsts.Function, CezFunctionConsts.GetUserByField),
                new(CezParamsConsts.RestFormat, "json"),
                new("field", request.Field),
                new("values[0]", request.Value)
            ]
        );

        if (requestResult.Error != null)
        {
            return new CezGetUserResponse
            {
                Success = false,
                ErrorCode = requestResult.Error.ErrorCode,
                Message = requestResult.Error.Message ?? requestResult.Error.Error
            };
        }

        var userDetail = requestResult.Data != null && requestResult.Data.Count > 0 ? requestResult.Data[0] : null;
        if (userDetail == null)
        {
            return new CezGetUserResponse
            {
                Success = false,
                Message = CezResponseMessageConsts.UnexpectedError
            };
        }

        return new CezGetUserResponse
        {
            Success = true,
            Data = mapper.Map<CezSiteInfo>(userDetail)
        };
    }

    public async Task<CezCourseContentResponse> GetCourseContent(CezCourseRequest request)
    {
        var externalRequest = new ExternalCezCourseRequest
        {
            Token = request.Token,
            Function = CezFunctionConsts.GetCourseContents,
            CourseId = request.CourseId
        };

        var requestResult = await requestService.SendGetAsync<List<ExternalCezCourseSection>>(
            CezBaseConsts.FunctionsPath,
            [
                new(CezParamsConsts.Token, externalRequest.Token),
                new(CezParamsConsts.Function, externalRequest.Function),
                new(CezParamsConsts.RestFormat, externalRequest.RestFormat),
                new(CezParamsConsts.CourseId, externalRequest.CourseId.ToString())
            ]
        );

        return mapper.Map<CezCourseContentResponse>(requestResult);
    }

    public async Task<Stream> DownloadCezFile(CezFileRequest request)
    {
        var externalRequest = new ExternalCezDownloadFileRequest
        {
            Token = request.Token,
            FileUrl = request.FileUrl
        };

        return await requestService.DownloadFileAsync(externalRequest.FileUrl, externalRequest.Token);
    }
}
