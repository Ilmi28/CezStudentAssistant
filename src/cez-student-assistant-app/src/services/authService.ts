import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";

export const authService = {
  async login(userName: string, password: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/auth/login`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, password }),
      },
      true
    );
    return handleResponse<void>(res, true);
  },

  async register(userName: string, password: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/auth/register`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, password }),
      },
      true
    );
    return handleResponse<void>(res, true);
  },

  async loginCez(userName: string, password: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/auth/login-cez`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, password }),
      },
      true
    );
    return handleResponse<void>(res, true);
  },

  async refreshToken(): Promise<void> {
    const res = await fetch(`${API_BASE_URL}/auth/refresh`, {
      method: "POST",
      credentials: "include",
    });
    return handleResponse<void>(res, true);
  },

  async logout(): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/auth/logout`,
      {
        method: "POST",
      },
      true
    );
    return handleResponse<void>(res, true);
  },
};
