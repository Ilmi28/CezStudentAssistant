import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type { UserConfigurationDto, UpdateUserConfigurationPayload, UserUsageDto, DashboardStatsDto, RecentActivityDto } from "../types";

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

  async getUserUsage(): Promise<UserUsageDto> {
    const res = await customFetch(
      `${API_BASE_URL}/user/usage`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<UserUsageDto>(res);
  },

  async getDashboardStats(): Promise<DashboardStatsDto> {
    const res = await customFetch(
      `${API_BASE_URL}/user/dashboard-stats`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<DashboardStatsDto>(res);
  },

  async getRecentActivity(limit: number = 6): Promise<RecentActivityDto[]> {
    const res = await customFetch(
      `${API_BASE_URL}/user/recent-activity?limit=${limit}`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<RecentActivityDto[]>(res);
  },
};


