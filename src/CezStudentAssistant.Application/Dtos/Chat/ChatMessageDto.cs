using CezStudentAssistant.Domain.Enums;
using System.Text.Json.Serialization;

namespace CezStudentAssistant.Application.Dtos.Chat;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ChatThreadId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChatMessageRole Role { get; set; }
    public required string Content { get; set; }
    public int TokenCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
