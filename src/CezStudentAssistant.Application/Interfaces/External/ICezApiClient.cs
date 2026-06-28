using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses.Cez;

namespace CezStudentAssistant.Application.Interfaces.External;

public interface ICezApiClient
{
    Task<CezLoginResponse> LoginToCez(CezLoginRequest loginDto);
    Task<CezGetUserCoursesResponse> GetUserCourses(CezUserRequest request);
    Task<CezGetSiteInfoResponse> GetSiteInfo(CezBaseRequest request);
    Task<CezCourseContentResponse> GetCourseContent(CezCourseRequest request);
    Task<Stream> DownloadCezFile(CezFileRequest request);
}
