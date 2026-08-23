namespace CezStudentAssistant.Application.Dtos.AI;

public class EstimateQuizTokensDto
{
    public int EstimatedTokens { get; set; }
    public int DailyTokenLimit { get; set; }
    public int DailyTokensUsed { get; set; }
    public int DailyTokensReserved { get; set; }
    public double EstimatedDailyUsagePercentage { get; set; }
    public int RemainingDailyTokens { get; set; }
    public bool CanGenerate { get; set; }
}
