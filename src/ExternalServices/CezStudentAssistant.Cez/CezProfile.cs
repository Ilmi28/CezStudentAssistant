using AutoMapper;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez.Consts;
using CezStudentAssistant.Cez.Responses;
using CezStudentAssistant.Cez.Services;

namespace CezStudentAssistant.Cez;

internal class CezProfile : Profile
{
    public CezProfile()
    {
        CreateMap<ExternalCezGetUserCoursesResponse, CezCourse>()
            .ForMember(
                dest => dest.ExternalId,
                opt => opt.MapFrom(src => src.Id)
            );

        CreateMap<ExternalGetSiteInfoResponse, CezSiteInfo>()
            .ForMember(dest => dest.ExternalUserId, opt => opt.MapFrom(src => src.UserId));

        CreateMap<ExternalCezLoginResponse, CezTokens>()
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token))
            .ForMember(dest => dest.PrivateToken, opt => opt.MapFrom(src => src.PrivateToken));

        CreateMap<CezRequestResult<List<ExternalCezGetUserCoursesResponse>>, CezGetUserCoursesResponse>()
            .ForMember(
                dest => dest.Success,
                opt => opt.MapFrom(src => src.Error == null && src.Data != null && src.Data.Count > 0)
            )
            .ForMember(dest => dest.ErrorCode, opt => opt.MapFrom(src => src.Error != null ? src.Error.ErrorCode : null))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Error != null ? src.Error.Message ?? src.Error.Error : null))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

        CreateMap<CezRequestResult<ExternalCezLoginResponse>, CezLoginResponse>()
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Error == null))
            .ForMember(dest => dest.ErrorCode, opt => opt.MapFrom(src => src.Error != null ? src.Error.ErrorCode : null))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Error != null ? MapLoginErrorCodeToMessage(src.Error.ErrorCode) : null))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data))
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<CezRequestResult<ExternalGetSiteInfoResponse>, CezGetSiteInfoResponse>()
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Error == null && src.Data != null))
            .ForMember(dest => dest.ErrorCode, opt => opt.MapFrom(src => src.Error != null ? src.Error.ErrorCode : null))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Error != null ? src.Error.Message ?? src.Error.Error : null))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

        CreateMap<CezRequestResult<List<ExternalCezCourseSection>>, CezCourseContentResponse>()
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Error == null && src.Data != null))
            .ForMember(dest => dest.ErrorCode, opt => opt.MapFrom(src => src.Error != null ? src.Error.ErrorCode : null))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Error != null ? src.Error.Message ?? src.Error.Error : null))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => FlattenCourseContents(src.Data)));
    }

    private static string MapLoginErrorCodeToMessage(string? errorCode) => errorCode switch
    {
        CezErrorConsts.MissingParam => CezResponseMessageConsts.MissingParam,
        CezErrorConsts.InvalidLogin => CezResponseMessageConsts.InvalidLogin,
        _ => CezResponseMessageConsts.UnexpectedError
    };

    private static List<CezCourseContent>? FlattenCourseContents(List<ExternalCezCourseSection>? sections)
    {
        return sections?
            .SelectMany(section => section.Modules)
            .SelectMany(module => module.Contents.Select(content => new CezCourseContent
            {
                FileName = content.FileName,
                Type = MapResourceType(content.Type),
                FileUrl = content.FileUrl,
                MimeType = content.MimeType ?? MimeTypes.GetMimeType(content.FileName),
                ModuleId = module.Id,
                TimeCreated = content.TimeCreated.HasValue ? DateTimeOffset.FromUnixTimeSeconds(content.TimeCreated.Value).UtcDateTime
                    : DateTimeOffset.FromUnixTimeSeconds(content.TimeModified).UtcDateTime,
                TimeModified = DateTimeOffset.FromUnixTimeSeconds(content.TimeModified).UtcDateTime
            }))
            .ToList();
    }

    private static CezResourceType MapResourceType(string type)
    {
        return type switch
        {
            "file" => CezResourceType.File,
            "url" => CezResourceType.Url,
            _ => CezResourceType.File
        };
    }
}