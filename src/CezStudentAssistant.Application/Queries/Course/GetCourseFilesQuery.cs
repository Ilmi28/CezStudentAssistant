using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Course;

public sealed class GetCourseFilesQuery : IQuery<List<CourseResourceDto>>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
}

public class GetCourseFilesQueryHandler(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IAIClient aiClient,
    IConfiguration configuration) : BaseQueryHandler<GetCourseFilesQuery, List<CourseResourceDto>>
{
    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? throw new InvalidOperationException(CourseMessageConsts.CourseFilesContainerConfigMissing);

    protected override string SuccessMessage => CourseMessageConsts.GetCourseDetailsSuccess;
    protected override string ErrorMessage => CourseMessageConsts.GetCourseDetailsError;

    protected override async Task<List<CourseResourceDto>> ExecuteAsync(GetCourseFilesQuery query, CancellationToken ct)
    {
        var courseRepository = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepository.GetByIdAsync(query.CourseId, ct, true, c => c.Users);

        if (course == null)
        {
            throw new NotFoundException(CourseMessageConsts.CourseNotFound);
        }

        if (!course.Users.Any(u => u.Id == query.UserId))
        {
            throw new UnauthorizedException(CourseMessageConsts.CourseAccessDenied);
        }

        var maxTokensConfig = configuration["Gemini:MaximumDailyTokens"];
        if (string.IsNullOrWhiteSpace(maxTokensConfig) || !int.TryParse(maxTokensConfig, out var dailyTokenLimit) || dailyTokenLimit <= 0)
        {
            throw new InvalidOperationException(UserMessageConsts.MaximumDailyTokensConfigMissing);
        }

        var resourceRepository = unitOfWork.Repository<ICezResourceRepository>();
        var resources = await resourceRepository.Find(r => r.CourseId == query.CourseId, false).ToListAsync(ct);

        var updatedAny = false;
        foreach (var resource in resources)
        {
            if (resource.EstimatedTokens <= 0)
            {
                await using var stream = await fileService.DownloadAsync($"{query.CourseId}/{resource.Name}", _containerName, ct);
                if (stream != null)
                {
                    var tokens = await aiClient.EstimateTokenUsageAsync(new AIQuizRequest
                    {
                        QuestionCount = 0,
                        Files = [new AIFile { Stream = stream, MimeType = resource.MimeType }]
                    });

                    if (tokens > 0)
                    {
                        resource.EstimatedTokens = tokens;
                        updatedAny = true;
                    }
                }
            }
        }

        if (updatedAny)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }

        return resources
            .OrderByDescending(r => r.EstimatedTokens)
            .ThenBy(r => r.DisplayName)
            .Select(r => new CourseResourceDto
            {
                Id = r.Id,
                DisplayName = r.DisplayName,
                MimeType = r.MimeType,
                LastModified = r.LastModifiedAt,
                DownloadUrl = $"/course/{course.Id}/file/{r.Id}/download",
                IsHidden = r.IsHidden,
                EstimatedTokens = r.EstimatedTokens,
                EstimatedDailyUsagePercentage = Math.Round((double)r.EstimatedTokens / dailyTokenLimit * 100, 2)
            }).ToList();
    }
}
