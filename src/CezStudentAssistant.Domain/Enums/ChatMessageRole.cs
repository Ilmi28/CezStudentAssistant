using System.Text.Json.Serialization;

namespace CezStudentAssistant.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatMessageRole
{
    User = 1,
    Assistant = 2
}
