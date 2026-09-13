import i18n from "../i18n";

export class ApiError extends Error {
  public statusCode: number;
  public errors: string[] | null;

  constructor(message: string, statusCode: number, errors: string[] | null = null) {
    super(message);
    this.name = "ApiError";
    this.statusCode = statusCode;
    this.errors = errors;
  }
}

export class UnauthorizedError extends ApiError {
  constructor(message = "Unauthorized access") {
    super(message, 401);
    this.name = "UnauthorizedError";
  }
}

export class PayloadTooLargeError extends ApiError {
  constructor(message = "File size limit exceeded") {
    super(message, 413);
    this.name = "PayloadTooLargeError";
  }
}

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

const parseJsonResponse = async <T>(res: Response): Promise<ApiResponse<T> | null> => {
  const contentType = res.headers.get("content-type");
  if (!contentType || !contentType.includes("application/json")) {
    return null;
  }
  try {
    return (await res.json()) as ApiResponse<T>;
  } catch (parseError) {
    console.warn("[baseClient] Failed to parse JSON response body:", parseError);
    return null;
  }
};

const defaultRefreshToken = async (): Promise<void> => {
  const res = await fetch(`${API_BASE_URL}/auth/refresh`, {
    method: "POST",
    credentials: "include",
  });

  if (!res.ok) {
    throw new UnauthorizedError();
  }

  const body = await parseJsonResponse<void>(res);

  if (body && !body.success) {
    throw new UnauthorizedError(body.message || "Unauthorized access");
  }
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

  if (res.status === 401 && !isAuthEndpoint) {
    const refresh = refreshTokenFn || defaultRefreshToken;
    if (!isRefreshing) {
      isRefreshing = true;
      try {
        await refresh();
        isRefreshing = false;
        onRefreshed(true);
        res = await fetch(input, options);
      } catch (refreshError) {
        isRefreshing = false;
        onRefreshed(false);
        console.warn("[baseClient] Token refresh failed:", refreshError);
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

const getTranslatedErrorMessage = (serverMessage: string | null | undefined): string => {
  if (!serverMessage) return i18n.t("common.genericError");

  const key = `apiErrors.${serverMessage}`;
  if (i18n.exists(key)) {
    return i18n.t(key);
  }

  return serverMessage;
};

export const handleResponse = async <T>(res: Response, isAuthEndpoint = false): Promise<T> => {
  const body = await parseJsonResponse<T>(res);

  if (res.status === 401 && !isAuthEndpoint) {
    throw new UnauthorizedError();
  }

  if (res.status === 413) {
    throw new PayloadTooLargeError(i18n.t("courseDetails.fileTooLargeError", "Rozmiar pliku jest za duży (przekroczono limit serwera)."));
  }

  if (!res.ok || (body && !body.success)) {
    const serverMessage = body?.message;

    if (isAuthEndpoint) {
      if (serverMessage === "Username already exists.") {
        throw new ApiError(i18n.t("auth.usernameTaken"), res.status, body?.errors);
      }
      
      const isCredentialError =
        serverMessage === "Invalid username or password." ||
        serverMessage === "Invalid username or password for CEZ login." ||
        serverMessage === "Invalid login credentials for CEZ.";

      if (isCredentialError) {
        throw new ApiError(i18n.t("auth.invalidCredentials"), res.status, body?.errors);
      }

      const translatedMessage = getTranslatedErrorMessage(serverMessage);
      throw new ApiError(translatedMessage, res.status, body?.errors);
    }

    if (serverMessage) {
      const translatedMessage = getTranslatedErrorMessage(serverMessage);
      throw new ApiError(translatedMessage, res.status, body?.errors);
    }

    throw new ApiError(i18n.t("common.serverError", "Wystąpił błąd podczas przetwarzania żądania."), res.status);
  }

  return body?.data as T;
};
