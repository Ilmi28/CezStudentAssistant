using AutoMapper;
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

    public async Task<CezGetSiteInfoResponse> GetSiteInfo(CezBaseRequest request)
    {
        var externalRequest = new ExternalCezBaseRequest
        {
            Token = request.Token,
            Function = CezFunctionConsts.GetSiteInfo,
        };
        var requestResult = await requestService.SendGetAsync<ExternalGetSiteInfoResponse>(
            CezBaseConsts.FunctionsPath,
            [
                new(CezParamsConsts.Token, externalRequest.Token),
                new(CezParamsConsts.Function, externalRequest.Function),
                new(CezParamsConsts.RestFormat, externalRequest.RestFormat)
            ]
        );

        return mapper.Map<CezGetSiteInfoResponse>(requestResult);
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
