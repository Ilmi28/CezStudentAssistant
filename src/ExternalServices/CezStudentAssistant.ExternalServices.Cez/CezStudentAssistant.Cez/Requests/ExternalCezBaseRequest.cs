namespace CezStudentAssistant.Cez.Requests;

internal class ExternalCezBaseRequest
{
    public required string Token { get; set; }
    public required string Function { get; set; }
    public string RestFormat { get; set; } = "json";
}
