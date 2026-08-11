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

export interface UploadCourseFileResponseDto {
  resourceId: string;
}

export interface GenerateQuizRequestDto {
  questionCount: number;
  additionalInstructions?: string;
}

export interface GenerateQuizResponseDto {
  quizId?: string;
  jobId?: string;
}
