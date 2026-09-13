import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type {
  FlashcardDeckDto,
  FlashcardDeckDetailsDto,
  EstimateFlashcardTokensDto,
  FlashcardAttemptDto,
} from "../types/flashcardTypes";
import type { PagedResultDto, PagedQueryParams } from "../types/commonTypes";
import { FlashcardStateEnum } from "../enums/flashcardEnums";

export const flashcardService = {
  async getFlashcardDecks(params?: PagedQueryParams | string): Promise<PagedResultDto<FlashcardDeckDto>> {
    let url = `${API_BASE_URL}/flashcards`;
    if (typeof params === "string") {
      url += `?courseId=${encodeURIComponent(params)}&pageSize=100`;
    } else if (params) {
      const queryParts: string[] = [];
      if (params.pageNumber) queryParts.push(`pageNumber=${params.pageNumber}`);
      if (params.pageSize) queryParts.push(`pageSize=${params.pageSize}`);
      if (params.searchTerm) queryParts.push(`searchTerm=${encodeURIComponent(params.searchTerm)}`);
      if (params.courseId) queryParts.push(`courseId=${encodeURIComponent(params.courseId)}`);
      if (queryParts.length > 0) {
        url += `?${queryParts.join("&")}`;
      }
    }
    const res = await customFetch(
      url,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<PagedResultDto<FlashcardDeckDto>>(res);
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
    hardCount?: number | null,
    generateFromPromptOnly?: boolean
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
          generateFromPromptOnly,
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
    additionalInstructions?: string,
    generateFromPromptOnly?: boolean
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
          generateFromPromptOnly,
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

  async submitFlashcardAttemptCardState(
    attemptId: string,
    cardId: string,
    state: FlashcardStateEnum
  ): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/attempt/${attemptId}/card/${cardId}/state`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ state }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async completeFlashcardAttempt(attemptId: string): Promise<FlashcardAttemptDto> {
    const res = await customFetch(
      `${API_BASE_URL}/flashcards/attempt/${attemptId}/complete`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
      },
      false,
      authService.refreshToken
    );
    return handleResponse<FlashcardAttemptDto>(res);
  },
};
