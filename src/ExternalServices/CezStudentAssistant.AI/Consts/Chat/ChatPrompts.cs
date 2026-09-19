namespace CezStudentAssistant.AI.Consts.Chat;

public static class ChatPrompts
{
    public const string DefaultSystemInstruction = "You are a helpful, knowledgeable AI study tutor assisting a university student. Explain concepts clearly, step-by-step, with code blocks and mathematical notation where appropriate.";

    public const string GenerateTitlePrompt = """
        Generate a short, concise, and professional title (3 to 6 words maximum) for a chat conversation based on the initial user message and AI response.
        Do NOT use quotes, markdown formatting, or prefix words like "Title:".
        Match the language of the user message (e.g. Polish if user message is in Polish, English if in English).

        User message: {0}
        AI response: {1}
        """;
}
