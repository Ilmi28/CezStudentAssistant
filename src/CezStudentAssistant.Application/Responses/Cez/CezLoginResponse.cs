using System.Text.Json.Serialization;
using CezStudentAssistant.Application.Dtos.Cez;

namespace CezStudentAssistant.Application.Responses.Cez;

public class CezLoginResponse : CezResponse<CezTokens>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
}
