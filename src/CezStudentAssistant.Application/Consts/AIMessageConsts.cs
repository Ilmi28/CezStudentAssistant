namespace CezStudentAssistant.Application.Consts;

public static class AIMessageConsts
{
    public const string QuizGenerationEnqueued = "Quiz generation enqueued successfully.";
    public const string QuizGenerationError = "Failed to generate quiz.";
    public const string QuizGenerationSuccess = "Successfully generated quiz.";
    public const string CourseNotFound = "Course not found.";
    public const string ResourceNotFound = "Resource not found.";
    public const string JobNotFound = "Job not found.";
    public const string DailyTokenLimitExceeded = "Daily token limit exceeded. Please try again tomorrow or generate a smaller quiz.";
    public const string EstimateQuizTokensSuccess = "Successfully estimated quiz tokens.";
    public const string EstimateQuizTokensError = "Failed to estimate quiz tokens.";
}
