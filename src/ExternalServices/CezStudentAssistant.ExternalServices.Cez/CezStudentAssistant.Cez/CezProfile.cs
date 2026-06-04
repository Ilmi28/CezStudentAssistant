using AutoMapper;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Cez.Consts;
using CezStudentAssistant.Cez.Responses;
using CezStudentAssistant.Cez.Services;
using System.Globalization;

namespace CezStudentAssistant.Cez;

internal class CezProfile : Profile
{
    public CezProfile()
    {
        CreateMap<ExternalCezGetUserCoursesResponse, CezCourse>()
            .ForMember(
                dest => dest.ExternalId,
                opt => opt.MapFrom(src => src.Id.ToString(CultureInfo.InvariantCulture))
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
    }

    private static string MapLoginErrorCodeToMessage(string? errorCode) => errorCode switch
    {
        CezErrorConsts.MissingParam => CezResponseMessageConsts.MissingParam,
        CezErrorConsts.InvalidLogin => CezResponseMessageConsts.InvalidLogin,
        _ => CezResponseMessageConsts.UnexpectedError
    };
}