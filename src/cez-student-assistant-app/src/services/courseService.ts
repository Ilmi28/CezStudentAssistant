import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type {
  CourseDto,
  CourseDetailsDto,
  UploadCourseFileResponseDto,
  GenerateQuizResponseDto,
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

  async generateQuiz(
    courseId: string,
    questionCount: number,
    additionalInstructions?: string
  ): Promise<GenerateQuizResponseDto> {
    const res = await customFetch(
      `${API_BASE_URL}/course/${courseId}/generate-quiz`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ questionCount, additionalInstructions }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<GenerateQuizResponseDto>(res);
  },

  async getDownloadFileUrl(courseId: string, fileId: string): Promise<string> {
    return `${API_BASE_URL}/course/${courseId}/file/${fileId}/download`;
  },
};
