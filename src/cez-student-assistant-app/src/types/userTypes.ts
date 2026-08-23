export type UserTheme = 1 | 2 | 3;
export const UserTheme = {
  Dark: 1 as UserTheme,
  Light: 2 as UserTheme,
  System: 3 as UserTheme,
} as const;

export type UserLanguage = 1 | 2;
export const UserLanguage = {
  Polish: 1 as UserLanguage,
  English: 2 as UserLanguage,
} as const;

export interface UserConfigurationDto {
  isCezConnected: boolean;
  theme: UserTheme;
  language: UserLanguage;
}

export interface UpdateUserConfigurationPayload {
  theme?: UserTheme;
  language?: UserLanguage;
}

export interface UserUsageDto {
  dailyTokensUsed: number;
  dailyTokensReserved?: number;
  dailyTokenLimit: number;
  dailyUsagePercentage: number;
}

