namespace CezStudentAssistant.AI.Consts.Quiz;

public static class QuizSchemas
{
    public const string PropertyTitle = "title";
    public const string PropertyDescription = "description";
    public const string PropertyQuestions = "questions";
    public const string PropertyContent = "content";
    public const string PropertyQuestionType = "questionType";
    public const string PropertyPoints = "points";
    public const string PropertyOptions = "options";
    public const string PropertyIsCorrect = "isCorrect";

    public const string QuizTitleDescription = "The title of the quiz summarizing its subject matter.";
    public const string QuizDescriptionDescription = "A brief summary of what the quiz covers.";
    public const string QuizQuestionsDescription = "The list of questions included in the quiz.";

    public const string QuestionContentDescription = "The actual question text.";
    public const string QuestionTypeDescriptionTemplate = "The format type of the question. Allowed values: {0}.";
    public const string QuestionPointsDescription = "The score value awarded for answering the question correctly.";
    public const string QuestionOptionsDescription = "List of answer choices for the question.";

    public const string OptionContentDescription = "The text of the option choice.";
    public const string OptionIsCorrectDescription = "True if this choice represents the correct answer, otherwise false.";
}
