using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using HeyRed.Mime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CezStudentAssistant.Application.Services;

public class CezService(
    ICezApiClient cezApiClient,
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IAIClient aiClient,
    IJobScheduler jobScheduler,
    IJobService jobService,
    IConfiguration configuration) : ICezService
{
    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? throw new InvalidOperationException(CourseMessageConsts.CourseFilesContainerConfigMissing);
    public async Task<Guid> LoginWithCezAsync(string userName, string password, CancellationToken ct = default)
    {
        var loginResponse = await cezApiClient.LoginToCez(new CezLoginRequest
        {
            UserName = userName,
            Password = password,
        });

        if (!loginResponse.Success || loginResponse.Data is null)
            throw new BadRequestException(loginResponse.Message ?? CezMessagesConsts.LoginError);

        var userInfoResponse = await cezApiClient.GetUser(new CezGetUserRequest
        {
            Token = loginResponse.Data.Token,
            Field = "username",
            Value = userName
        });

        if (!userInfoResponse.Success || userInfoResponse.Data is null)
            throw new BadRequestException(userInfoResponse.Message ?? CezMessagesConsts.GetUserError);

        var cezUserInfo = new CezUserInfo
        {
            SiteInfo = userInfoResponse.Data,
            Tokens = loginResponse.Data
        };

        return await SyncCezUser(cezUserInfo, ct);
    }

    public async Task ConnectCezAsync(Guid userId, string userName, string password, CancellationToken ct = default)
    {
        var cezUserRepo = unitOfWork.Repository<ICezUserRepository>();
        var existingCezUser = await cezUserRepo.GetSingleAsync(cu => cu.UserId == userId, ct);

        if (existingCezUser != null && !existingCezUser.IsDisabled)
        {
            throw new ConflictException(CezMessagesConsts.AlreadyConnectedToCez);
        }

        var loginResponse = await cezApiClient.LoginToCez(new CezLoginRequest
        {
            UserName = userName,
            Password = password,
        });

        if (!loginResponse.Success || loginResponse.Data is null)
            throw new BadRequestException(loginResponse.Message ?? CezMessagesConsts.LoginError);

        var userInfoResponse = await cezApiClient.GetUser(new CezGetUserRequest
        {
            Token = loginResponse.Data.Token,
            Field = "username",
            Value = userName
        });

        if (!userInfoResponse.Success || userInfoResponse.Data is null)
            throw new BadRequestException(userInfoResponse.Message ?? CezMessagesConsts.GetUserError);

        var externalUserConnected = await cezUserRepo.GetSingleAsync(cu => cu.ExternalUserId == userInfoResponse.Data.ExternalUserId, ct);

        if (externalUserConnected != null && externalUserConnected.UserId != userId && !externalUserConnected.IsDisabled)
        {
            throw new ConflictException(CezMessagesConsts.CezAccountAlreadyLinkedToAnotherUser);
        }

        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(userId, ct);
        if (user != null)
        {
            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(userInfoResponse.Data.FullName))
                user.FullName = userInfoResponse.Data.FullName;
            if (string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(userInfoResponse.Data.Email))
                user.Email = userInfoResponse.Data.Email;
        }

        if (existingCezUser != null)
        {
            existingCezUser.IsDisabled = false;
            existingCezUser.UserName = userInfoResponse.Data.UserName ?? userName;
            existingCezUser.FullName = userInfoResponse.Data.FullName;
            existingCezUser.Email = userInfoResponse.Data.Email;
            existingCezUser.Token = loginResponse.Data.Token ?? string.Empty;
            existingCezUser.PrivateToken = loginResponse.Data.PrivateToken ?? string.Empty;
            existingCezUser.ExternalUserId = userInfoResponse.Data.ExternalUserId;
        }
        else
        {
            var cezUser = new CezUser
            {
                UserId = userId,
                UserName = userInfoResponse.Data.UserName ?? userName,
                FullName = userInfoResponse.Data.FullName,
                Email = userInfoResponse.Data.Email,
                Token = loginResponse.Data.Token ?? string.Empty,
                PrivateToken = loginResponse.Data.PrivateToken ?? string.Empty,
                ExternalUserId = userInfoResponse.Data.ExternalUserId,
                IsDisabled = false
            };
            await cezUserRepo.AddAsync(cezUser, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DisconnectCezAsync(Guid userId, CancellationToken ct = default)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(userId, ct, includes: u => u.CezUser!)
            ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        if (user.CezUser == null || user.CezUser.IsDisabled)
        {
            throw new BadRequestException(CezMessagesConsts.NotConnectedToCez);
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new BadRequestException(CezMessagesConsts.CannotDisconnectPureCezAccount);
        }

        user.CezUser.IsDisabled = true;
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SyncUserCourses(Guid userId, CancellationToken ct = default)
    {
        var job = await jobService.GetLatestJobAsync(userId, JobType.CezSync, ct);
        if (job == null)
            throw new AppException(CezMessagesConsts.SyncCoursesError);

        await jobService.UpdateJobAsync(job, JobStatus.Processing, ct: ct);
        try
        {
            var cezUser = await GetCezUserAsync(userId, ct);

            var userInfoResponse = await cezApiClient.GetUser(new CezGetUserRequest
            {
                Token = cezUser.Token,
                Field = "id",
                Value = cezUser.ExternalUserId.ToString()
            });

            if (userInfoResponse != null && userInfoResponse.Success && userInfoResponse.Data != null)
            {
                if (!string.IsNullOrWhiteSpace(userInfoResponse.Data.FullName))
                    cezUser.FullName = userInfoResponse.Data.FullName;
                if (!string.IsNullOrWhiteSpace(userInfoResponse.Data.Email))
                    cezUser.Email = userInfoResponse.Data.Email;
                if (!string.IsNullOrWhiteSpace(userInfoResponse.Data.UserName))
                    cezUser.UserName = userInfoResponse.Data.UserName;
            }

            var externalCourses = await FetchExternalCoursesAsync(cezUser, ct);
            var localUser = await GetUserWithCoursesAsync(userId, ct);

            if (userInfoResponse != null && userInfoResponse.Success && userInfoResponse.Data != null)
            {
                if (string.IsNullOrWhiteSpace(localUser.FullName) && !string.IsNullOrWhiteSpace(userInfoResponse.Data.FullName))
                    localUser.FullName = userInfoResponse.Data.FullName;
                if (string.IsNullOrWhiteSpace(localUser.Email) && !string.IsNullOrWhiteSpace(userInfoResponse.Data.Email))
                    localUser.Email = userInfoResponse.Data.Email;
            }

            await SynchronizeCoursesAsync(localUser, externalCourses, ct);

            EnqueueCourseSyncJobs(cezUser.Token, externalCourses, ct);

            await jobService.UpdateJobAsync(job, JobStatus.Succeeded, ct: ct);
        }
        catch
        {
            await jobService.UpdateJobAsync(job, JobStatus.Failed, ct: ct);
            throw;
        }
    }

    private async Task<CezUser> GetCezUserAsync(Guid userId, CancellationToken ct)
    {
        var repo = unitOfWork.Repository<ICezUserRepository>();
        return await repo.GetSingleAsync(cu => cu.UserId == userId && !cu.IsDisabled, ct)
            ?? throw new NotFoundException(CezMessagesConsts.CezUserNotFound);
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
            throw new BadRequestException(response.Message ?? CezMessagesConsts.GetUserCoursesError);

        return response.Data.ToList();
    }

    private async Task<User> GetUserWithCoursesAsync(Guid userId, CancellationToken ct)
    {
        var repo = unitOfWork.Repository<IUserRepository>();
        return await repo.GetByIdAsync(userId, ct, includes: x => x.Courses)
            ?? throw new NotFoundException(CezMessagesConsts.CezUserNotFound);
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
            throw new BadRequestException(courseContentResponse.Message ?? CezMessagesConsts.GetCourseContentsError);

        if (courseContentResponse.Data is null || courseContentResponse.Data.Count == 0)
            return;

        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetSingleAsync(x => x.CezExternalId == courseRequest.CourseId, cancellationToken)
                    ?? throw new NotFoundException(CezMessagesConsts.CezCourseNotFound);

        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var fileContents = courseContentResponse.Data.Where(x => x.Type == Enums.CezResourceType.File);
        foreach (var content in fileContents)
        {
            var contentName = $"{content.ModuleId}_{((DateTimeOffset)content.TimeCreated).ToUnixTimeSeconds()}.{MimeTypesMap.GetExtension(content.MimeType)}";
            var existingResource = await resourceRepo.GetSingleAsync(r => r.Name == contentName, cancellationToken);
            if (existingResource == null || existingResource.CezLastModified != content.TimeModified)
            {
                var fileContent = await cezApiClient.DownloadCezFile(new CezFileRequest
                {
                    Token = courseRequest.Token,
                    FileUrl = content.FileUrl
                });

                using var memoryStream = new MemoryStream();
                if (fileContent.CanSeek) fileContent.Position = 0;
                await fileContent.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                await fileService.UploadAsync(memoryStream, $"{course.Id}/{contentName}", _containerName, content.MimeType, cancellationToken);

                memoryStream.Position = 0;
                var estimatedTokens = await aiClient.EstimateTokenUsageAsync(new CezStudentAssistant.Application.Requests.AI.AIQuizRequest
                {
                    QuestionCount = 0,
                    Files = [new CezStudentAssistant.Application.Requests.AI.AIFile { Stream = memoryStream, MimeType = content.MimeType }]
                });

                if (existingResource != null)
                {
                    existingResource.DisplayName = content.FileName;
                    existingResource.CezLastModified = content.TimeModified;
                    existingResource.MimeType = content.MimeType;
                    existingResource.EstimatedTokens = estimatedTokens;
                }
                else
                {
                    var newResource = new Resource
                    {
                        Name = contentName,
                        DisplayName = content.FileName,
                        CezLastModified = content.TimeModified,
                        MimeType = content.MimeType,
                        EstimatedTokens = estimatedTokens,
                        CourseId = course.Id
                    };
                    await resourceRepo.AddAsync(newResource, cancellationToken);
                }
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
                    ?? throw new BadRequestException(CezMessagesConsts.GetUserError),
                FullName = siteInfoData.FullName,
                Email = siteInfoData.Email
            };

            await cezUserRepo.AddAsync(
                new CezUser
                {
                    UserName = siteInfoData.UserName,
                    FullName = siteInfoData.FullName,
                    Email = siteInfoData.Email,
                    Token = tokensData.Token ?? string.Empty,
                    PrivateToken = tokensData.PrivateToken ?? string.Empty,
                    ExternalUserId = siteInfoData.ExternalUserId,
                    User = user
                },
                ct
            );

            return user.Id;
        }

        if (string.IsNullOrWhiteSpace(existingUser.FullName) && !string.IsNullOrWhiteSpace(siteInfoData.FullName))
            existingUser.FullName = siteInfoData.FullName;
        if (string.IsNullOrWhiteSpace(existingUser.Email) && !string.IsNullOrWhiteSpace(siteInfoData.Email))
            existingUser.Email = siteInfoData.Email;

        var existingCezUser = await cezUserRepo.GetSingleAsync(cu => cu.UserId == existingUser.Id, ct);

        if (existingCezUser == null)
        {
            existingCezUser = new CezUser
            {
                UserName = siteInfoData.UserName,
                FullName = siteInfoData.FullName,
                Email = siteInfoData.Email,
                Token = tokensData.Token ?? string.Empty,
                PrivateToken = tokensData.PrivateToken ?? string.Empty,
                ExternalUserId = siteInfoData.ExternalUserId,
                UserId = existingUser.Id,
                IsDisabled = false
            };
            await cezUserRepo.AddAsync(existingCezUser, ct);
            return existingUser.Id;
        }

        existingCezUser.IsDisabled = false;
        existingCezUser.UserName = siteInfoData.UserName;
        existingCezUser.FullName = siteInfoData.FullName;
        existingCezUser.Email = siteInfoData.Email;
        existingCezUser.Token = tokensData.Token ?? string.Empty;
        existingCezUser.PrivateToken = tokensData.PrivateToken ?? string.Empty;
        existingCezUser.ExternalUserId = siteInfoData.ExternalUserId;

        await unitOfWork.SaveChangesAsync(ct);

        return existingUser.Id;
    }
}
