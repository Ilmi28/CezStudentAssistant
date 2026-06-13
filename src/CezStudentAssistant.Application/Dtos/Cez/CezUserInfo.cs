namespace CezStudentAssistant.Application.Dtos.Cez;

public class CezUserInfo
{
    public required CezSiteInfo SiteInfo { get; set; }
    public required CezTokens Tokens { get; set; }
}
