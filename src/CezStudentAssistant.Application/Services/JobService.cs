using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Services;

public class JobService(IUnitOfWork unitOfWork, IJobNotificationService notificationService) : IJobService, IScopedService
{
    public async Task<Job?> GetLatestJobAsync(Guid userId, JobType type, CancellationToken ct = default)
    {
        var repo = unitOfWork.Repository<IJobRepository>();
        return await repo.Find(j => j.UserId == userId && j.Type == type)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Job> CreateJobAsync(Guid userId, JobType type, CancellationToken ct = default)
    {
        var job = new Job
        {
            UserId = userId,
            JobId = "pending",
            Status = JobStatus.Enqueued,
            Type = type
        };

        var repo = unitOfWork.Repository<IJobRepository>();
        await repo.AddAsync(job, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return job;
    }

    public async Task UpdateJobAsync(Job job, JobStatus status, string? hangfireJobId = null, CancellationToken ct = default)
    {
        if (hangfireJobId != null)
        {
            job.JobId = hangfireJobId;
        }
        job.Status = status;
        await unitOfWork.SaveChangesAsync(ct);
        await notificationService.SendJobStatusUpdateAsync(job.UserId, job.JobId, status, ct);
    }
}
