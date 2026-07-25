namespace CezStudentAssistant.AI.Consts;

public static class AIErrorMessages
{
    public const string EmptyResponseErrorMessage = "AI returned an empty response.";
    public const string DeserializationErrorMessage = "Failed to deserialize the quiz structure returned by AI.";
    public const string GeneralErrorMessageFormat = "An error occurred while generating the quiz: {0}";
    public const string DefaultModelConfigMissing = "Gemini:DefaultModel configuration is missing.";
}
