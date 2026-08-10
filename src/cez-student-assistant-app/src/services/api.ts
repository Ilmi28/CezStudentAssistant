import i18n from "../i18n";

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string | null;
  data: T | null;
  errors: string[] | null;
}

export interface CourseDto {
  id: string;
  name: string;
  lastSynched: string | null;
}

export interface CourseResourceDto {
  id: string;
  displayName: string;
  mimeType: string;
  lastModified: string;
  downloadUrl: string;
}

export interface CourseDetailsDto {
  id: string;
  name: string;
  description: string | null;
  type: number;
  lastSynched: string | null;
  files: CourseResourceDto[];
}

export interface QuizDto {
  id: string;
  name: string;
  displayName: string;
  courseId: string;
  courseName: string;
}

export interface QuestionOptionDto {
  id: string;
  content: string;
  isCorrect?: boolean;
}

export interface QuestionDto {
  id: string;
  content: string;
  type: number;
  points: number;
  options: QuestionOptionDto[];
}

export interface QuizDetailsDto {
  id: string;
  name: string;
  displayName: string;
  courseId: string;
  courseName: string;
  questions: QuestionDto[];
}

export interface UserConfigurationDto {
  isCezConnected: boolean;
  theme: number;
  language: number;
}

export interface CezStatusDto {
  isConnected: boolean;
  lastSyncAt: string | null;
  lastSyncStatus: number | null;
}

const API_BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:8081";

const handleResponse = async <T>(res: Response, isAuthEndpoint = false): Promise<T> => {
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

export const api = {
  // Auth
  async login(userName: string, password: string): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName, password }),
      credentials: "include",
    });
    return handleResponse(res, true);
  },

  async register(userName: string, password: string): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/auth/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName, password }),
      credentials: "include",
    });
    return handleResponse(res, true);
  },

  async loginCez(userName: string, password: string): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/auth/login-cez`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ userName, password }),
      credentials: "include",
    });
    return handleResponse(res, true);
  },

  async logout(): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/auth/logout`, {
      method: "POST",
      credentials: "include",
    });
    return handleResponse(res, true);
  },

  // Cez Sync
  async syncCourses(): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/cez/sync-courses`, {
      method: "POST",
      credentials: "include",
    });
    return handleResponse(res);
  },

  // Courses
  async getCourses(): Promise<CourseDto[]> {
    const res = await fetch(`${API_BASE_URL}/course`, {
      method: "GET",
      credentials: "include",
    });
    return handleResponse<CourseDto[]>(res);
  },

  async getCourseDetails(id: string): Promise<CourseDetailsDto> {
    const res = await fetch(`${API_BASE_URL}/course/${id}`, {
      method: "GET",
      credentials: "include",
    });
    return handleResponse<CourseDetailsDto>(res);
  },

  async uploadCourseFile(courseId: string, file: File): Promise<any> {
    const formData = new FormData();
    formData.append("file", file);

    const res = await fetch(`${API_BASE_URL}/course/file/upload?courseId=${courseId}`, {
      method: "POST",
      body: formData,
      credentials: "include",
    });
    return handleResponse(res);
  },

  async generateQuiz(courseId: string, questionCount: number, additionalInstructions?: string): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/course/${courseId}/generate-quiz`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ questionCount, additionalInstructions }),
      credentials: "include",
    });
    return handleResponse(res);
  },

  async getDownloadFileUrl(courseId: string, fileId: string): Promise<string> {
    return `${API_BASE_URL}/course/${courseId}/file/${fileId}/download`;
  },

  // Quizzes
  async getQuizzes(): Promise<QuizDto[]> {
    const res = await fetch(`${API_BASE_URL}/quiz`, {
      method: "GET",
      credentials: "include",
    });
    return handleResponse<QuizDto[]>(res);
  },

  async getQuizDetails(id: string): Promise<QuizDetailsDto> {
    const res = await fetch(`${API_BASE_URL}/quiz/${id}`, {
      method: "GET",
      credentials: "include",
    });
    return handleResponse<QuizDetailsDto>(res);
  },

  async submitAnswer(quizAttemptId: string, questionId: string, questionOptionIds: string[]): Promise<any> {
    const res = await fetch(`${API_BASE_URL}/quiz/answer`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ quizAttemptId, questionId, questionOptionIds }),
      credentials: "include",
    });
    return handleResponse(res);
  },

  // User
  async getUserConfiguration(): Promise<UserConfigurationDto> {
    const res = await fetch(`${API_BASE_URL}/user/configuration`, {
      method: "GET",
      credentials: "include",
    });
    return handleResponse<UserConfigurationDto>(res);
  },

  // CEZ Status
  async getCezStatus(): Promise<CezStatusDto> {
    const res = await fetch(`${API_BASE_URL}/cez/status`, {
      method: "GET",
      credentials: "include",
    });
    return handleResponse<CezStatusDto>(res);
  },
};
