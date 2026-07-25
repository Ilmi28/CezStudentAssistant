using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IJobService
{
    Task<Job?> GetLatestJobAsync(Guid userId, JobType type, CancellationToken ct = default);
    Task<Job> CreateJobAsync(Guid userId, JobType type, CancellationToken ct = default);
    Task UpdateJobAsync(Job job, JobStatus status, string? hangfireJobId = null, CancellationToken ct = default);
}
