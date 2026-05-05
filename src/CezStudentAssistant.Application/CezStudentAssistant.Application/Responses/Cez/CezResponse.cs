namespace CezStudentAssistant.Application.Responses.Cez;

public class CezResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public T? Data { get; set; }
}
