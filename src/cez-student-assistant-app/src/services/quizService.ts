import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type {
  QuizDto,
  QuizDetailsDto,
  SubmitAnswerResponseDto,
} from "../types";

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

  async submitAnswer(
    quizAttemptId: string,
    questionId: string,
    questionOptionIds: string[]
  ): Promise<SubmitAnswerResponseDto> {
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
    return handleResponse<SubmitAnswerResponseDto>(res);
  },
};
