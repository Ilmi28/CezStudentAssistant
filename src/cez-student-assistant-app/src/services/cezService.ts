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

  async connectCez(username: string, password: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/cez/connect`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      },
      false,
      authService.refreshToken
    );
    await handleResponse<void>(res);
  },

  async disconnectCez(): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/cez/disconnect`,
      { method: "POST" },
      false,
      authService.refreshToken
    );
    await handleResponse<void>(res);
  },
};
