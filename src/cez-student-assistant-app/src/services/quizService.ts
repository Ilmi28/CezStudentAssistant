import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";

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

export const quizService = {
  async getQuizzes(): Promise<QuizDto[]> {
    const res = await customFetch(
      `${API_BASE_URL}/quiz`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<QuizDto[]>(res);
  },

  async getQuizDetails(id: string): Promise<QuizDetailsDto> {
    const res = await customFetch(
      `${API_BASE_URL}/quiz/${id}`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<QuizDetailsDto>(res);
  },

  async submitAnswer(quizAttemptId: string, questionId: string, questionOptionIds: string[]): Promise<any> {
    const res = await customFetch(
      `${API_BASE_URL}/quiz/answer`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ quizAttemptId, questionId, questionOptionIds }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse(res);
  },
};
