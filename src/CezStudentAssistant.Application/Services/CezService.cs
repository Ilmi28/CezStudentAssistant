using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using HeyRed.Mime;

namespace CezStudentAssistant.Application.Services;

public class CezService(ICezApiClient cezApiClient, IUnitOfWork unitOfWork, IFileService fileService, IJobScheduler jobScheduler) : ICezService
{
    public async Task<Guid> LoginWithCezAsync(string userName, string password, CancellationToken ct = default)
    {
        var loginResponse = await cezApiClient.LoginToCez(new CezLoginRequest
        {
            UserName = userName,
            Password = password,
        });

        if (!loginResponse.Success || loginResponse.Data is null)
            throw new BadRequestException(new ApiMessage(this, loginResponse.Message ?? CezMessagesConsts.LoginError));

        var userInfoResponse = await cezApiClient.GetSiteInfo(new CezBaseRequest
        {
            Token = loginResponse.Data.Token
        });

        if (!userInfoResponse.Success || userInfoResponse.Data is null)
            throw new BadRequestException(new ApiMessage(this, userInfoResponse.Message ?? CezMessagesConsts.GetSiteInfoError));

        var cezUserInfo = new CezUserInfo
        {
            SiteInfo = userInfoResponse.Data,
            Tokens = loginResponse.Data
        };

        return await SyncCezUser(cezUserInfo, ct);
    }

    public async Task SyncUserCourses(Guid userId, CancellationToken ct = default)
    {
        var cezUserRepo = unitOfWork.Repository<ICezUserRepository>();
        var cezUser = await cezUserRepo.GetSingleAsync(cu => cu.UserId == userId, ct)
            ?? throw new NotFoundException(new ApiMessage(this, CezMessagesConsts.CezUserNotFound));

        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var userCoursesResponse = await cezApiClient.GetUserCourses(new CezUserRequest
        {
            Token = cezUser.Token,
            UserId = cezUser.ExternalUserId
        });

        if (!userCoursesResponse.Success || userCoursesResponse.Data is null)
            throw new BadRequestException(new ApiMessage(this, userCoursesResponse.Message ?? CezMessagesConsts.GetUserCoursesError));

        var userRepo = unitOfWork.Repository<IUserRepository>();

        var user = await userRepo.GetByIdAsync(userId, ct, includes: x => x.Courses)
            ?? throw new NotFoundException(new ApiMessage(this, CezMessagesConsts.CezUserNotFound));

        var userCourses = userCoursesResponse.Data;
        var incomingExternalIds = userCourses.Select(c => c.ExternalId).ToList();
        var existingGlobalCourses = await courseRepo.FindAsync(c => c.CezExternalId != null && incomingExternalIds.Contains(c.CezExternalId.Value), ct);
        var globalCoursesMap = existingGlobalCourses
            .Where(c => c.CezExternalId.HasValue)
            .ToDictionary(c => c.CezExternalId!.Value);

        foreach (var course in userCourses)
        {
            globalCoursesMap.TryGetValue(course.ExternalId, out var existingCourse);

            if (existingCourse == null)
            {
                var newCourse = new Course
                {
                    Users = new List<User> { user },
                    CezExternalId = course.ExternalId,
                    Name = course.DisplayName!,
                    Type = Domain.Enums.CourseType.Cez
                };

                await courseRepo.AddAsync(newCourse, ct);
                globalCoursesMap[course.ExternalId] = newCourse;
            }
            else
            {
                existingCourse.Name = course.DisplayName ?? existingCourse.Name;
                existingCourse.LastSynched = DateTime.UtcNow;

                if (!user.Courses.Any(c => c.CezExternalId == course.ExternalId))
                {
                    user.Courses.Add(existingCourse);
                }
            }

            var cezCourseRequest = new CezCourseRequest
            {
                Token = cezUser.Token,
                CourseId = course.ExternalId
            };
            jobScheduler.Enqueue<ICezService>(job => job.SyncCourseContent(cezCourseRequest, ct));
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SyncCourseContent(CezCourseRequest courseRequest, CancellationToken cancellationToken)
    {
        var courseContentResponse = await cezApiClient.GetCourseContent(courseRequest);

        if (!courseContentResponse.Success)
            throw new BadRequestException(new ApiMessage(this, courseContentResponse.Message ?? CezMessagesConsts.GetCourseContentsError));

        if (courseContentResponse.Data is null || courseContentResponse.Data.Count == 0)
            return;

        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetSingleAsync(x => x.CezExternalId == courseRequest.CourseId, cancellationToken)
                    ?? throw new NotFoundException(new ApiMessage(this, CezMessagesConsts.CezCourseNotFound));

        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var fileContents = courseContentResponse.Data.Where(x => x.Type == Enums.CezResourceType.File);
        foreach (var content in fileContents)
        {
            var contentName = $"{content.ModuleId}_{((DateTimeOffset)content.TimeCreated).ToUnixTimeSeconds()}.{MimeTypesMap.GetExtension(content.MimeType)}";
            var existingResource = await resourceRepo.GetSingleAsync(r => r.Name == contentName, cancellationToken);
            if (existingResource == null || existingResource.CezLastModified != content.TimeModified)
            {
                var newResource = new CezResource
                {
                    Name = contentName,
                    DisplayName = content.FileName,
                    CezLastModified = content.TimeModified,
                    MimeType = content.MimeType,
                    CourseId = course.Id
                };
                await resourceRepo.AddAsync(newResource, cancellationToken);
                var fileContent = await cezApiClient.DownloadCezFile(new CezFileRequest
                {
                    Token = courseRequest.Token,
                    FileUrl = content.FileUrl
                });
                await fileService.UploadAsync(fileContent, $"{course.Id}/{contentName}", ContainerNameConsts.CourseFilesContainer, content.MimeType, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> SyncCezUser(CezUserInfo cezUserInfo, CancellationToken ct = default)
    {
        var cezUserRepo = unitOfWork.Repository<ICezUserRepository>();
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var siteInfoData = cezUserInfo.SiteInfo;
        var tokensData = cezUserInfo.Tokens;

        var existingUser = await userRepo.GetSingleAsync(u => u.UserName == siteInfoData.UserName, ct);
        if (existingUser == null)
        {
            var user = new User
            {
                UserName = siteInfoData.UserName
                    ?? throw new BadRequestException(new ApiMessage(this, CezMessagesConsts.GetSiteInfoError))
            };

            await cezUserRepo.AddAsync(
                new CezUser
                {
                    FullName = siteInfoData.FullName,
                    Token = tokensData.Token ?? string.Empty,
                    PrivateToken = tokensData.PrivateToken ?? string.Empty,
                    ExternalUserId = siteInfoData.ExternalUserId,
                    User = user
                },
                ct
            );

            return user.Id;
        }

        var existingCezUser = await cezUserRepo.GetSingleAsync(cu => cu.UserId == existingUser.Id, ct);
        if (existingCezUser == null)
        {
            existingCezUser = new CezUser
            {
                FullName = siteInfoData.FullName,
                Token = tokensData.Token ?? string.Empty,
                PrivateToken = tokensData.PrivateToken ?? string.Empty,
                ExternalUserId = siteInfoData.ExternalUserId,
                UserId = existingUser.Id
            };
            await cezUserRepo.AddAsync(existingCezUser, ct);
            return existingUser.Id;
        }

        existingCezUser.FullName = siteInfoData.FullName;
        existingCezUser.Token = tokensData.Token ?? string.Empty;
        existingCezUser.PrivateToken = tokensData.PrivateToken ?? string.Empty;
        existingCezUser.ExternalUserId = siteInfoData.ExternalUserId;

        await unitOfWork.SaveChangesAsync(ct);

        return existingUser.Id;
    }
}
