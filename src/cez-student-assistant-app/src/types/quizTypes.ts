import { QuizStatusEnum, QuestionDifficulty } from '../enums/quizEnums';

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
  points: number;
  options: QuestionOptionDto[];
}

export interface QuizDetailsDto {
  id: string;
  name: string;
  displayName: string;
  courseId: string;
  courseName: string;
  questions: QuestionDto[];
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

export { QuizStatusEnum, QuizAttemptStatus, QuestionType, QuestionDifficulty } from '../enums/quizEnums';


