import { API_BASE_URL, customFetch, handleResponse, UnauthorizedError } from "./baseClient";

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

    if (!res.ok) {
      throw new UnauthorizedError();
    }

    let body: any = null;
    const contentType = res.headers.get("content-type");
    if (contentType && contentType.includes("application/json")) {
      try {
        body = await res.json();
      } catch (parseError) {
        console.warn("[authService] Failed to parse refresh token JSON response:", parseError);
      }
    }

    if (body && !body.success) {
      throw new UnauthorizedError(body.message || "Unauthorized access");
    }
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

  async setPassword(newPassword: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/auth/set-password`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ newPassword }),
      },
      false,
      authService.refreshToken
    );
    await handleResponse<void>(res);
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/auth/change-password`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ currentPassword, newPassword }),
      },
      false,
      authService.refreshToken
    );
    await handleResponse<void>(res);
  },
};
