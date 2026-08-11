import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type { CezStatusDto, CezSyncResponseDto } from "../types";

export const cezService = {
  async getCezStatus(): Promise<CezStatusDto> {
    const res = await customFetch(
      `${API_BASE_URL}/cez/status`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<CezStatusDto>(res);
  },

  async syncCourses(): Promise<CezSyncResponseDto> {
    const res = await customFetch(
      `${API_BASE_URL}/cez/sync-courses`,
      { method: "POST" },
      false,
      authService.refreshToken
    );
    return handleResponse<CezSyncResponseDto>(res);
  },
};
