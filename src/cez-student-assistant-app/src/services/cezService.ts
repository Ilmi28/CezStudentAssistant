import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";

export interface CezStatusDto {
  isConnected: boolean;
  lastSyncAt: string | null;
  lastSyncStatus: number | null;
}

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

  async syncCourses(): Promise<any> {
    const res = await customFetch(
      `${API_BASE_URL}/cez/sync-courses`,
      { method: "POST" },
      false,
      authService.refreshToken
    );
    return handleResponse(res);
  },
};
