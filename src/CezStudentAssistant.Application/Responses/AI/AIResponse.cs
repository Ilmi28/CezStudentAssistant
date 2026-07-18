namespace CezStudentAssistant.Application.Responses.AI;

public class AIResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
