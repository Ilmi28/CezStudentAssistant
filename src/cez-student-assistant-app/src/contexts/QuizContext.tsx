import { createContext, useState, type ReactNode } from "react";
import type { QuizDto } from "../services";

export interface QuizContextState {
  quizzes: QuizDto[];
  setQuizzes: React.Dispatch<React.SetStateAction<QuizDto[]>>;
}

export const QuizContext = createContext<QuizContextState | null>(null);

export function QuizProvider({ children }: { children: ReactNode }) {
  const [quizzes, setQuizzes] = useState<QuizDto[]>([]);

  return (
    <QuizContext.Provider value={{ quizzes, setQuizzes }}>
      {children}
    </QuizContext.Provider>
  );
}
