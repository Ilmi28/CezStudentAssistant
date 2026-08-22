import { QuizStatusEnum, QuizAttemptStatus, QuestionDifficulty } from '../enums/quizEnums';

export interface QuizDto {
  id: string;
  name: string;
  displayName: string;
  courseId: string;
  courseName: string;
  status?: QuizStatusEnum;
}

export interface QuestionOptionDto {
  id: string;
  content: string;
  isCorrect?: boolean;
}

export interface QuestionDto {
  id: string;
  content: string;
  type: number;
  difficulty?: QuestionDifficulty;
  options: QuestionOptionDto[];
}

export interface QuestionAnswerDto {
  id: string;
  questionId: string;
  selectedOptionIds: string[];
}

export interface QuizAttemptDto {
  id: string;
  userId: string;
  quizId: string;
  status: QuizAttemptStatus;
  points?: number | null;
  startedAt: string;
  expiresAt?: string | null;
  answers: QuestionAnswerDto[];
}

export interface QuizDetailsDto {
  id: string;
  userId?: string;
  name: string;
  displayName: string;
  courseId: string;
  courseName: string;
  questions: QuestionDto[];
  attempts: QuizAttemptDto[];
}

export interface SubmitAnswerRequestDto {
  quizAttemptId: string;
  questionId: string;
  questionOptionIds: string[];
}

export interface SubmitAnswerResponseDto {
  isCorrect?: boolean;
  score?: number;
}

export interface QuizAttemptDetailsDto {
  attemptId: string;
  quizId: string;
  displayName: string;
  courseName: string;
  status: QuizAttemptStatus;
  isPending: boolean;
  points?: number | null;
  startedAt: string;
  expiresAt?: string | null;
  questions: QuestionDto[];
  answers: QuestionAnswerDto[];
}

export { QuizStatusEnum, QuizAttemptStatus, QuestionType, QuestionDifficulty } from '../enums/quizEnums';


