using CezStudentAssistant.Domain.Enums;
using System;

namespace CezStudentAssistant.Application.Dtos.Cez;

public class CezStatusDto
{
    public bool IsConnected { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public JobStatus? LastSyncStatus { get; set; }
}
