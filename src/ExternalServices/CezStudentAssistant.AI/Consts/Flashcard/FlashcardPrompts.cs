namespace CezStudentAssistant.AI.Consts.Flashcard;

public static class FlashcardPrompts
{
    public const string InstructionPromptTemplate = """
        You are an expert educational and study assistant. Your task is to generate a comprehensive, high-quality set of flashcards based STRICTLY and EXCLUSIVELY on the educational materials, book chapters, slides, or documents provided in the context.

        CRITICAL GROUNDING & ACCURACY RULES:
        1. Every flashcard (front key concept/question and back definition/explanation/answer) MUST be directly sourced from the provided document content (e.g. plot events, characters, definitions, theories, formulas, and facts from the text).
        2. NEVER generate flashcards about the system prompt, instructions, JSON schema, data structures, formatting, or unrelated general knowledge.
        3. The flashcard deck title and description MUST reflect the actual subject/topic of the generated cards (and strictly respect any specific sub-topic, chapter, or focus requested by the user).
        4. You must generate exactly {0} flashcards.
        5. For each flashcard, accurately classify its difficulty level:
           - "Easy": Direct factual recall, character/term identification, or basic definitions from the text.
           - "Medium": Conceptual understanding, plot development, applying rules, or multi-step reasoning from the text.
           - "Hard": Deep analysis, thematic interpretation, nuanced details, or synthesis across the text.
        6. The entire flashcard deck (title, description, front and back contents) must be written in the following language: {1}.
        """;

    public const string InstructionPromptFromPromptOnlyTemplate = """
        You are an expert educational and study assistant. Your task is to generate a comprehensive, high-quality set of flashcards based EXCLUSIVELY on the user's requested topic and custom instructions, using expert educational and domain knowledge. Do NOT rely on or expect any attached document files.

        CRITICAL ACCURACY & FOCUS RULES:
        1. Every flashcard (front key concept/question and back definition/explanation/answer) MUST be factually accurate and directly aligned with the user's specified topic and custom instructions.
        2. NEVER generate flashcards about the system prompt, instructions, JSON schema, data structures, formatting, or meta-instructions.
        3. The flashcard deck title and description MUST reflect the actual subject/topic of the generated cards.
        4. You must generate exactly {0} flashcards.
        5. For each flashcard, accurately classify its difficulty level:
           - "Easy": Direct factual recall or basic definitions for the subject.
           - "Medium": Conceptual understanding, applying rules, or multi-step reasoning.
           - "Hard": Deep analysis, nuanced details, or advanced problem solving.
        6. The entire flashcard deck (title, description, front and back contents) must be written in the following language: {1}.
        """;

    public const string DifficultyBreakdownTemplate = """

        [SYSTEM CRITICAL - Flashcard Difficulty Distribution]:
        You MUST generate the flashcards adhering to this exact difficulty distribution:
        - "Easy" flashcards: exactly {0}
        - "Medium" flashcards: exactly {1}
        - "Hard" flashcards: exactly {2}
        Total flashcards generated must be exactly {3}.
        """;

    public const string AdditionalInstructionsTemplate = """

        [SYSTEM CRITICAL - User Topic & Custom Focus Instructions]:
        The user has provided explicit instructions and custom focus requirements for this generation:
        ---
        {0}
        ---
        CRITICAL DIRECTIVE: You MUST strictly honor the user's custom instructions above (such as focusing on a specific chapter, topic, theme, event, or card style) while maintaining absolute factual accuracy to the provided document. The flashcard deck title, description, and card topics MUST directly reflect the user's requested focus topic when specified. (Do NOT ignore or suppress these instructions; they define the primary focus of the requested deck).
        """;
}
