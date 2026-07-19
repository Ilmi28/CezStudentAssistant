namespace CezStudentAssistant.AI.Consts;

internal static class GeminiAISchemas
{
    // Property Names
    public const string PropertyContent = "Content";
    public const string PropertyIsCorrect = "IsCorrect";
    public const string PropertyQuestionType = "QuestionType";
    public const string PropertyPoints = "Points";
    public const string PropertyOptions = "Options";
    public const string PropertyTitle = "Title";
    public const string PropertyDescription = "Description";
    public const string PropertyQuestions = "Questions";

    // Descriptions
    public const string OptionContentDescription = "The text content of the option.";
    public const string OptionIsCorrectDescription = "Whether this option is the correct answer.";
    public const string QuestionContentDescription = "The text of the question.";
    public const string QuestionTypeDescriptionTemplate = "The type of the question. Allowed values: {0}.";
    public const string QuestionPointsDescription = "The amount of points awarded for the correct answer.";
    public const string QuestionOptionsDescription = "The list of options for the question.";
    public const string QuizTitleDescription = "The title of the quiz.";
    public const string QuizDescriptionDescription = "A brief description of the quiz.";
    public const string QuizQuestionsDescription = "The list of questions in the quiz.";
}
