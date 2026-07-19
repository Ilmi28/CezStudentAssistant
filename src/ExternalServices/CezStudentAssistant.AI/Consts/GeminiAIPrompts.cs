namespace CezStudentAssistant.AI.Consts;

internal static class GeminiAIPrompts
{
    public const string InstructionPromptTemplate = """
        You are an expert educational assistant. Your task is to generate a high-quality quiz based on the provided content.
        You must generate exactly {0} questions.
        All questions must be relevant to the provided files or context.
        The entire quiz (title, description, questions, and options) must be written in the following language: {1}.
        """;
    public const string AdditionalInstructionsTemplate = "\nAdditional instructions to follow:\n{0}";
}
