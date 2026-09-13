import { useContext } from "react";
import { useTranslation } from "react-i18next";
import { FlashcardContext } from "../contexts/FlashcardContext";
import { flashcardService } from "../services/flashcardService";
import { UnauthorizedError } from "../services/baseClient";
import type { PagedQueryParams, PagedResultDto, FlashcardDeckDto } from "../types";
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

  const refreshDecks = async (params?: PagedQueryParams): Promise<PagedResultDto<FlashcardDeckDto> | null> => {
    try {
      const queryParams: PagedQueryParams = params
        ? { ...params, courseId: params.courseId || courseId }
        : { courseId };
      const result = await flashcardService.getFlashcardDecks(queryParams);
      ctx.setDecks(result.items);
      return result;
    } catch (err: unknown) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        throw err;
      }
      setError(t("common.errorConnection"));
      return null;
    }
  };

  return {
    decks: ctx.decks,
    setDecks: ctx.setDecks,
    refreshDecks,
  };
}
