import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type {
  CourseDto,
  CourseDetailsDto,
  UploadCourseFileResponseDto,
  GenerateQuizResponseDto,
  EstimateQuizTokensResponseDto,
} from "../types";

export const courseService = {
  async getCourses(): Promise<CourseDto[]> {
    const res = await customFetch(
      `${API_BASE_URL}/course`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<CourseDto[]>(res);
  },

  async getCourseDetails(id: string): Promise<CourseDetailsDto> {
    const res = await customFetch(
      `${API_BASE_URL}/course/${id}`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<CourseDetailsDto>(res);
  },

  async uploadCourseFile(courseId: string, file: File): Promise<UploadCourseFileResponseDto> {
    const formData = new FormData();
    formData.append("file", file);

    const res = await customFetch(
      `${API_BASE_URL}/course/file/upload?courseId=${courseId}`,
      {
        method: "POST",
        body: formData,
      },
      false,
      authService.refreshToken
    );
    return handleResponse<UploadCourseFileResponseDto>(res);
  },

  async estimateQuizTokens(
    courseId: string,
    questionCount: number,
    additionalInstructions?: string
  ): Promise<EstimateQuizTokensResponseDto> {
    const res = await customFetch(
      `${API_BASE_URL}/course/${courseId}/estimate-quiz-tokens`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ questionCount, additionalInstructions }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<EstimateQuizTokensResponseDto>(res);
  },

  async generateQuiz(
    courseId: string,
    questionCount: number,
    timeLimitMinutes?: number | null,
    additionalInstructions?: string
  ): Promise<GenerateQuizResponseDto> {
    const res = await customFetch(
      `${API_BASE_URL}/course/${courseId}/generate-quiz`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ questionCount, timeLimitMinutes, additionalInstructions }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<GenerateQuizResponseDto>(res);
  },

  async getDownloadFileUrl(courseId: string, fileId: string): Promise<string> {
    return `${API_BASE_URL}/course/${courseId}/file/${fileId}/download`;
  },

  async downloadCourseFile(courseId: string, fileId: string, fileName: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/course/${courseId}/file/${fileId}/download`,
      { method: "GET" },
      false,
      authService.refreshToken
    );

    if (!res.ok) {
      throw new Error("Failed to download file.");
    }

    const blob = await res.blob();
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  },

  async createCourse(name: string, description?: string): Promise<string> {
    const res = await customFetch(
      `${API_BASE_URL}/course`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, description }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<string>(res);
  },

  async updateCourse(courseId: string, name: string, description?: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/course`,
      {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ courseId, name, description }),
      },
      false,
      authService.refreshToken
    );
    await handleResponse(res);
  },

  async deleteCourse(courseId: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/course/${courseId}`,
      { method: "DELETE" },
      false,
      authService.refreshToken
    );
    await handleResponse(res);
  },

  async deleteCourseFile(courseId: string, fileId: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/course/${courseId}/file/${fileId}`,
      { method: "DELETE" },
      false,
      authService.refreshToken
    );
    await handleResponse(res);
  },
};
