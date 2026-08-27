import { FlashcardDeckStatusEnum, FlashcardStateEnum } from "../enums/flashcardEnums";
import { QuestionDifficulty, QuizAttemptStatus } from "../enums/quizEnums";

export interface FlashcardDto {
  id: string;
  front: string;
  back: string;
  difficulty: QuestionDifficulty;
  state: FlashcardStateEnum;
  deckId: string;
}

export interface FlashcardAttemptDto {
  id: string;
  userId: string;
  deckId: string;
  status: QuizAttemptStatus;
  cardCount: number;
  masteredCount: number;
  learningCount: number;
  progressPercentage: number;
  startedAt: string;
  completedAt?: string | null;
}

export interface FlashcardDeckDto {
  id: string;
  userId: string;
  name: string;
  courseId: string;
  courseName: string;
  status: FlashcardDeckStatusEnum;
  cardCount: number;
  masteredCardCount: number;
  learningCardCount: number;
  newCardCount: number;
  progressPercentage: number;
}

export interface FlashcardDeckDetailsDto {
  id: string;
  userId: string;
  name: string;
  courseId: string;
  courseName: string;
  status: FlashcardDeckStatusEnum;
  cardCountPerAttempt?: number | null;
  easyCardCountPerAttempt?: number | null;
  mediumCardCountPerAttempt?: number | null;
  hardCardCountPerAttempt?: number | null;
  cardCount: number;
  masteredCardCount: number;
  learningCardCount: number;
  newCardCount: number;
  progressPercentage: number;
  cards: FlashcardDto[];
  attempts: FlashcardAttemptDto[];
}

export interface EstimateFlashcardTokensDto {
  estimatedTokens: number;
  dailyTokenLimit: number;
  dailyTokensUsed: number;
  dailyTokensReserved: number;
  estimatedDailyUsagePercentage: number;
  remainingDailyTokens: number;
  canGenerate: boolean;
}

export { FlashcardDeckStatusEnum, FlashcardStateEnum } from "../enums/flashcardEnums";
