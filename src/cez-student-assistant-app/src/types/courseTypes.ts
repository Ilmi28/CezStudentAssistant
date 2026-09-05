export interface CourseDto {
  id: string;
  name: string;
  lastSynched: string | null;
  isCez?: boolean;
}

export interface CourseResourceDto {
  id: string;
  displayName: string;
  mimeType: string;
  lastModified: string;
  downloadUrl: string;
  isHidden?: boolean;
  estimatedTokens?: number;
  estimatedDailyUsagePercentage?: number;
}

export interface CourseDetailsDto {
  id: string;
  name: string;
  description: string | null;
  type: number;
  lastSynched: string | null;
  files: CourseResourceDto[];
  isCez?: boolean;
  preparationPercentage?: number | null;
  quizProgressPercentage?: number | null;
  flashcardProgressPercentage?: number | null;
}

export interface UploadCourseFileResponseDto {
  resourceId: string;
}

export interface GenerateQuizRequestDto {
  questionCount: number;
  timeLimitMinutes?: number | null;
  additionalInstructions?: string;
}

export interface GenerateQuizResponseDto {
  quizId?: string;
  jobId?: string;
}

export interface EstimateQuizTokensRequestDto {
  questionCount: number;
  additionalInstructions?: string;
}

export interface EstimateQuizTokensResponseDto {
  estimatedTokens: number;
  dailyTokenLimit: number;
  dailyTokensUsed: number;
  dailyTokensReserved?: number;
  estimatedDailyUsagePercentage: number;
  remainingDailyTokens: number;
  canGenerate: boolean;
}

export { CourseType } from '../enums/courseEnums';

