import { useContext } from "react";
import { useTranslation } from "react-i18next";
import { FlashcardContext } from "../contexts/FlashcardContext";
import { flashcardService } from "../services/flashcardService";
import { UnauthorizedError } from "../services/baseClient";
import { useAuth } from "./useAuth";
import { useUI } from "./useUI";

export function useFlashcards(courseId?: string) {
  const ctx = useContext(FlashcardContext);
  if (!ctx) {
    throw new Error("useFlashcards must be used within a FlashcardProvider");
  }

  const { t } = useTranslation();
  const { handleLogout } = useAuth();
  const { setError } = useUI();



  const refreshDecks = async () => {
    try {
      const deckList = await flashcardService.getFlashcardDecks(courseId);
      ctx.setDecks(deckList);
    } catch (err: unknown) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        throw err;
      }
      setError(t("common.errorConnection"));
    }
  };

  return {
    decks: ctx.decks,
    setDecks: ctx.setDecks,
    refreshDecks,
  };
}
