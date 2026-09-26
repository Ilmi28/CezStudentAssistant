namespace CezStudentAssistant.Application.Consts;

public static class QuizMessageConsts
{
    public const string GetQuizzesSuccess = "Successfully retrieved list of quizzes.";
    public const string GetQuizzesError = "An error occurred while retrieving quizzes.";
    public const string GetQuizSuccess = "Successfully retrieved quiz details.";
    public const string GetQuizError = "An error occurred while retrieving quiz details.";
    public const string GetQuizAttemptSuccess = "Successfully retrieved quiz attempt details.";
    public const string GetQuizAttemptError = "An error occurred while retrieving quiz attempt details.";
    public const string QuizNotFound = "Requested quiz not found.";

    public const string AnswerSubmittedSuccess = "Answer submitted successfully.";
    public const string AnswerSubmittedError = "An error occurred while submitting the answer.";

    public const string QuizAttemptNotFound = "Quiz attempt not found.";
    public const string QuestionNotFound = "Question not found.";
    public const string QuestionOptionNotFound = "Question option not found.";

    public const string UnauthorizedAttemptAccess = "You are not authorized to submit answers for this quiz attempt.";
    public const string QuizAttemptNotInProgress = "This quiz attempt is not currently in progress.";
    public const string QuizAttemptExpired = "This quiz attempt has expired.";
    public const string QuestionNotBelongToQuiz = "This question does not belong to the quiz being attempted.";
    public const string QuestionAlreadyAnswered = "An answer has already been submitted for this question.";
    public const string SingleChoiceMultipleOptions = "Only one option can be selected for a single choice question.";

    public const string Forbidden = "You are not allowed to submit an answer for this quiz attempt.";

    public const string StartQuizSuccess = "Quiz started successfully.";
    public const string StartQuizError = "An error occurred while starting the quiz.";
    public const string UnauthorizedStartAccess = "You are not authorized to start this quiz attempt.";
    public const string QuizAttemptNotReady = "This quiz attempt is not ready to be started.";

    public const string CompleteQuizAttemptSuccess = "Quiz attempt completed successfully.";
    public const string CompleteQuizAttemptError = "An error occurred while completing the quiz attempt.";

    public const string UpdateQuizSuccess = "Quiz updated successfully.";
    public const string UpdateQuizError = "An error occurred while updating the quiz.";
    public const string QuizAccessDenied = "You are not authorized to modify this quiz.";

    public const string DeleteQuizSuccess = "Quiz deleted successfully.";
    public const string DeleteQuizError = "An error occurred while deleting the quiz.";
}
