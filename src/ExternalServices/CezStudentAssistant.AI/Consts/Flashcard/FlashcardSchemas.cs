namespace CezStudentAssistant.AI.Consts.Flashcard;

public static class FlashcardSchemas
{
    public const string PropertyTitle = "title";
    public const string PropertyDescription = "description";
    public const string PropertyCards = "cards";
    public const string PropertyFront = "front";
    public const string PropertyBack = "back";
    public const string PropertyDifficulty = "difficulty";

    public const string DeckTitleDescription = "The title of the flashcard deck summarizing its subject matter.";
    public const string DeckDescriptionDescription = "A brief summary of what the flashcard deck covers.";
    public const string DeckCardsDescription = "The list of flashcards included in the deck.";

    public const string CardFrontDescription = "Key concept, term, or question on front of card.";
    public const string CardBackDescription = "Value, definition, or answer on back of card.";
    public const string CardDifficultyDescriptionTemplate = "Card difficulty level. Allowed values: {0}.";
}
