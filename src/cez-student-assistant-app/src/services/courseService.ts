import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";

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

  async uploadCourseFile(courseId: string, file: File): Promise<any> {
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
    return handleResponse(res);
  },

  async generateQuiz(courseId: string, questionCount: number, additionalInstructions?: string): Promise<any> {
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
    return handleResponse(res);
  },

  async getDownloadFileUrl(courseId: string, fileId: string): Promise<string> {
    return `${API_BASE_URL}/course/${courseId}/file/${fileId}/download`;
  },
};
