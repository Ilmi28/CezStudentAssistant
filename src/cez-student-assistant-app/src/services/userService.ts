import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";

export interface UserConfigurationDto {
  isCezConnected: boolean;
  theme: number;
  language: number;
}

export const userService = {
  async getUserConfiguration(): Promise<UserConfigurationDto> {
    const res = await customFetch(
      `${API_BASE_URL}/user/configuration`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<UserConfigurationDto>(res);
  },
};
