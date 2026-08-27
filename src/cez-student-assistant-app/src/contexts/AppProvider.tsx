import type { ReactNode } from "react";
import { UIProvider } from "./UIContext";
import { AuthProvider } from "./AuthContext";
import { CourseProvider } from "./CourseContext";
import { QuizProvider } from "./QuizContext";
import { FlashcardProvider } from "./FlashcardContext";

const providers = [UIProvider, AuthProvider, CourseProvider, QuizProvider, FlashcardProvider];

export function AppProvider({ children }: { children: ReactNode }) {
  return providers.reduceRight(
    (acc, Provider) => <Provider>{acc}</Provider>,
    children
  );
}
