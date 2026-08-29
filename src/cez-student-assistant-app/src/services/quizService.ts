import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type {
  QuizDto,
  QuizDetailsDto,
  QuizAttemptDetailsDto,
  SubmitAnswerResponseDto,
} from "../types";

export const quizService = {
  async getQuizzes(courseId?: string): Promise<QuizDto[]> {
    const url = courseId
      ? `${API_BASE_URL}/quiz?courseId=${encodeURIComponent(courseId)}`
      : `${API_BASE_URL}/quiz`;
    const res = await customFetch(
      url,
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

  async startQuiz(quizId: string): Promise<QuizAttemptDetailsDto> {
    const res = await customFetch(
      `${API_BASE_URL}/quiz/${quizId}/start`,
      { method: "POST" },
      false,
      authService.refreshToken
    );
    return handleResponse<QuizAttemptDetailsDto>(res);
  },

  async getQuizAttempt(attemptId: string): Promise<QuizAttemptDetailsDto> {
    const res = await customFetch(
      `${API_BASE_URL}/quiz/attempt/${attemptId}`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<QuizAttemptDetailsDto>(res);
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

  async completeQuizAttempt(attemptId: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/quiz/attempt/${attemptId}/complete`,
      { method: "POST" },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async updateQuiz(
    quizId: string,
    name: string,
    timeLimitMinutes?: number | null,
    questionCountPerAttempt?: number | null,
    easyQuestionCountPerAttempt?: number | null,
    mediumQuestionCountPerAttempt?: number | null,
    hardQuestionCountPerAttempt?: number | null
  ): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/quiz/${quizId}`,
      {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name,
          timeLimitMinutes,
          questionCountPerAttempt,
          easyQuestionCountPerAttempt,
          mediumQuestionCountPerAttempt,
          hardQuestionCountPerAttempt,
        }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },
};

