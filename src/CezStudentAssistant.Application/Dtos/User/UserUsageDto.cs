namespace CezStudentAssistant.Application.Dtos.User;

public class UserUsageDto
{
    public int DailyTokensUsed { get; set; }
    public int DailyTokensReserved { get; set; }
    public int DailyTokenLimit { get; set; }
    public double DailyUsagePercentage { get; set; }
}
