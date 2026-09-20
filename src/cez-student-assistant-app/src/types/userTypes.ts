import { UserLanguage, UserTheme } from "../enums/userEnums";
import { ActivityTypeEnum } from "../enums/activityEnums";

export { UserLanguage, UserTheme };

export interface UserConfigurationDto {
  userId: string;
  theme: UserTheme;
  language: UserLanguage;
  isCezConnected: boolean;
  hasPassword?: boolean;
  lastCezSync?: string | null;
}

export interface UpdateUserConfigurationPayload {
  theme?: UserTheme;
  language?: UserLanguage;
}

export interface UserProfileDto {
  userName: string;
  fullName?: string | null;
  email?: string | null;
  isCezConnected: boolean;
  cezUsername?: string | null;
  cezFullName?: string | null;
  cezEmail?: string | null;
}

export interface UpdateUserProfilePayload {
  userName: string;
  fullName?: string | null;
  email?: string | null;
}

export interface UserUsageDto {
  userId: string;
  monthYear: string;
  tokenCount: number;
  dailyTokenLimit?: number;
  dailyTokensUsed?: number;
  dailyTokensReserved?: number;
  dailyUsagePercentage?: number;
}

export interface DashboardStatsDto {
  courseCount: number;
  quizCount: number;
  flashcardDeckCount: number;
  flashcardCount: number;
  chatCount: number;
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
