import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type {
  FlashcardDeckDto,
  FlashcardDeckDetailsDto,
  EstimateFlashcardTokensDto,
  FlashcardAttemptDto,
} from "../types/flashcardTypes";
import { FlashcardStateEnum } from "../enums/flashcardEnums";

export const flashcardService = {
  async getFlashcardDecks(courseId?: string): Promise<FlashcardDeckDto[]> {
    const url = courseId
      ? `${API_BASE_URL}/flashcards?courseId=${courseId}`
      : `${API_BASE_URL}/flashcards`;
    const res = await customFetch(
      url,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<FlashcardDeckDto[]>(res);
  },

  async getFlashcardDeckDetails(id: string): Promise<FlashcardDeckDetailsDto> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/${id}`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<FlashcardDeckDetailsDto>(res);
  },

  async generateFlashcards(
    courseId: string,
    cardCount: number = 10,
    additionalInstructions?: string,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null
  ): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/generate`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          courseId,
          cardCount,
          additionalInstructions,
          easyCount,
          mediumCount,
          hardCount,
        }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async estimateFlashcardTokens(
    courseId: string,
    cardCount: number = 10,
    additionalInstructions?: string
  ): Promise<EstimateFlashcardTokensDto> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/course/${courseId}/estimate-tokens`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          courseId,
          cardCount,
          additionalInstructions,
        }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<EstimateFlashcardTokensDto>(res);
  },

  async updateFlashcardDeck(
    deckId: string,
    name: string,
    cardCountPerAttempt?: number | null,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null
  ): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/${deckId}`,
      {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name,
          cardCountPerAttempt,
          easyCardCountPerAttempt: easyCount,
          mediumCardCountPerAttempt: mediumCount,
          hardCardCountPerAttempt: hardCount,
        }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async deleteFlashcardDeck(deckId: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/${deckId}`,
      { method: "DELETE" },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async updateFlashcardState(
    cardId: string,
    state: FlashcardStateEnum
  ): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/card/${cardId}/state`,
      {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ state }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async resetFlashcardDeckProgress(deckId: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/${deckId}/reset`,
      { method: "POST" },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async startFlashcardAttempt(deckId: string, cardCount: number): Promise<FlashcardAttemptDto> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/${deckId}/attempt`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ cardCount }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<FlashcardAttemptDto>(res);
  },

  async completeFlashcardAttempt(
    attemptId: string,
    masteredCount: number,
    learningCount: number
  ): Promise<FlashcardAttemptDto> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/attempt/${attemptId}/complete`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ masteredCount, learningCount }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<FlashcardAttemptDto>(res);
  },
};
