import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type { UserConfigurationDto, UpdateUserConfigurationPayload } from "../types";

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

  async updateUserConfiguration(payload: UpdateUserConfigurationPayload): Promise<UserConfigurationDto> {
    const res = await customFetch(
      `${API_BASE_URL}/user/configuration`,
      {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<UserConfigurationDto>(res);
  },
};
