import i18n from "../i18n";

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string | null;
  data: T | null;
  errors: string[] | null;
}

export const API_BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:8081";

let isRefreshing = false;
let refreshSubscribers: ((success: boolean) => void)[] = [];

const onRefreshed = (success: boolean) => {
  refreshSubscribers.forEach((cb) => cb(success));
  refreshSubscribers = [];
};

export const customFetch = async (
  input: RequestInfo | URL,
  init?: RequestInit,
  isAuthEndpoint = false,
  refreshTokenFn?: () => Promise<any>
): Promise<Response> => {
  const options: RequestInit = {
    ...init,
    credentials: "include",
  };

  let res = await fetch(input, options);

  if (res.status === 401 && !isAuthEndpoint && refreshTokenFn) {
    if (!isRefreshing) {
      isRefreshing = true;
      try {
        await refreshTokenFn();
        isRefreshing = false;
        onRefreshed(true);
        res = await fetch(input, options);
      } catch {
        isRefreshing = false;
        onRefreshed(false);
      }
    } else {
      const refreshed = await new Promise<boolean>((resolve) => {
        refreshSubscribers.push(resolve);
      });
      if (refreshed) {
        res = await fetch(input, options);
      }
    }
  }

  return res;
};

export const handleResponse = async <T>(res: Response, isAuthEndpoint = false): Promise<T> => {
  let body: ApiResponse<T> | null = null;
  try {
    body = await res.json();
  } catch {
    // Non-JSON response
  }

  if (res.status === 401 && !isAuthEndpoint) {
    throw new Error("UNAUTHORIZED");
  }

  if (!res.ok || (body && !body.success)) {
    const serverMessage = body?.message;

    if (isAuthEndpoint) {
      if (serverMessage === "Username already exists.") {
        throw new Error(i18n.t("auth.usernameTaken") || "Użytkownik o podanej nazwie już istnieje.");
      }
      throw new Error(i18n.t("auth.invalidCredentials") || "Nieprawidłowa nazwa użytkownika lub hasło.");
    }

    if (serverMessage) {
      throw new Error(serverMessage);
    }

    throw new Error(`Request failed with status ${res.status}`);
  }

  return body?.data as T;
};
