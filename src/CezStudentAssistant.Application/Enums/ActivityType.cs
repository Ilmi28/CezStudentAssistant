using System.Text.Json.Serialization;

namespace CezStudentAssistant.Application.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActivityType
{
    Quiz,
    Flashcard,
    Chat
}
