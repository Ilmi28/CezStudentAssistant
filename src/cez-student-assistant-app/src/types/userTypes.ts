import { UserLanguage, UserTheme } from "../enums/userEnums";
import { ActivityTypeEnum } from "../enums/activityEnums";

export interface UserConfigurationDto {
  userId: string;
  theme: UserTheme;
  language: UserLanguage;
  isCezConnected: boolean;
  lastCezSync?: string | null;
}

export interface UpdateUserConfigurationPayload {
  theme?: UserTheme;
  language?: UserLanguage;
}

export interface UserUsageDto {
  userId: string;
  monthYear: string;
  tokenCount: number;
}

export interface DashboardStatsDto {
  courseCount: number;
  quizCount: number;
  flashcardDeckCount: number;
  flashcardCount: number;
}

export interface RecentActivityDto {
  id: string;
  entityId: string;
  type: ActivityTypeEnum;
  title: string;
  courseName: string;
  scorePercentage?: number | null;
  earnedPoints?: number | null;
  maxPoints?: number | null;
  masteredCount?: number | null;
  totalCount?: number | null;
  attemptDate: string;
  status: string;
}
