using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using HeyRed.Mime;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Application.Services;

public class CezService(
    ICezApiClient cezApiClient,
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IJobScheduler jobScheduler,
    IJobNotificationService notificationService) : ICezService
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
        var cezJobRepo = unitOfWork.Repository<ICezSyncJobRepository>();
        var job = await cezJobRepo.Find(j => j.UserId == userId).OrderByDescending(j => j.CreatedAt).FirstOrDefaultAsync(ct);
        if (job == null)
            throw new AppException(new ApiMessage(null, CezMessagesConsts.SyncCoursesError));

        await UpdateJobStatusAsync(userId, job, JobStatus.Processing, ct);
        try
        {
            var cezUser = await GetCezUserAsync(userId, ct);
            var externalCourses = await FetchExternalCoursesAsync(cezUser, ct);
            var localUser = await GetUserWithCoursesAsync(userId, ct);

            await SynchronizeCoursesAsync(localUser, externalCourses, ct);

            EnqueueCourseSyncJobs(cezUser.Token, externalCourses, ct);

            await UpdateJobStatusAsync(userId, job, JobStatus.Succeeded, ct);
        }
        catch
        {
            await UpdateJobStatusAsync(userId, job, JobStatus.Failed, ct);
            throw;
        }
    }

    private async Task UpdateJobStatusAsync(Guid userId, CezSyncJob job, JobStatus status, CancellationToken ct)
    {
        job.Status = status;
        await unitOfWork.SaveChangesAsync(ct);

        await notificationService.SendJobStatusUpdateAsync(userId, job.JobId, status, ct);
    }

    private async Task<CezUser> GetCezUserAsync(Guid userId, CancellationToken ct)
    {
        var repo = unitOfWork.Repository<ICezUserRepository>();
        return await repo.GetSingleAsync(cu => cu.UserId == userId, ct)
            ?? throw new NotFoundException(new ApiMessage(this, CezMessagesConsts.CezUserNotFound));
    }

    private async Task<List<CezCourse>> FetchExternalCoursesAsync(CezUser cezUser, CancellationToken ct)
    {
        var request = new CezUserRequest
        {
            Token = cezUser.Token,
            UserId = cezUser.ExternalUserId
        };

        var response = await cezApiClient.GetUserCourses(request);

        if (!response.Success || response.Data is null)
            throw new BadRequestException(new ApiMessage(this, response.Message ?? CezMessagesConsts.GetUserCoursesError));

        return response.Data.ToList();
    }

    private async Task<User> GetUserWithCoursesAsync(Guid userId, CancellationToken ct)
    {
        var repo = unitOfWork.Repository<IUserRepository>();
        return await repo.GetByIdAsync(userId, ct, includes: x => x.Courses)
            ?? throw new NotFoundException(new ApiMessage(this, CezMessagesConsts.CezUserNotFound));
    }

    private async Task SynchronizeCoursesAsync(User user, List<CezCourse> externalCourses, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var incomingExternalIds = externalCourses.Select(c => c.ExternalId).ToList();

        var existingGlobalCourses = await courseRepo
            .Find(c => c.CezExternalId != null && incomingExternalIds.Contains(c.CezExternalId.Value))
            .ToListAsync(ct);

        var globalCoursesMap = existingGlobalCourses
            .Where(c => c.CezExternalId.HasValue)
            .ToDictionary(c => c.CezExternalId!.Value);

        foreach (var course in externalCourses)
        {
            if (!globalCoursesMap.TryGetValue(course.ExternalId, out var existingCourse))
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
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private void EnqueueCourseSyncJobs(string token, List<CezCourse> externalCourses, CancellationToken ct)
    {
        foreach (var course in externalCourses)
        {
            var request = new CezCourseRequest
            {
                Token = token,
                CourseId = course.ExternalId
            };
            jobScheduler.Enqueue<ICezService>(job => job.SyncCourseContent(request, ct));
        }
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
                if (existingResource != null)
                {
                    existingResource.DisplayName = content.FileName;
                    existingResource.CezLastModified = content.TimeModified;
                    existingResource.MimeType = content.MimeType;
                }
                else
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
                }
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
