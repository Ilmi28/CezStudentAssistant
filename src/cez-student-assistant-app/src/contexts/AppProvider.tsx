import type { ReactNode } from "react";
import { UIProvider } from "./UIContext";
import { AuthProvider } from "./AuthContext";
import { CourseProvider } from "./CourseContext";
import { QuizProvider } from "./QuizContext";

const providers = [UIProvider, AuthProvider, CourseProvider, QuizProvider];

export function AppProvider({ children }: { children: ReactNode }) {
  return providers.reduceRight(
    (acc, Provider) => <Provider>{acc}</Provider>,
    children
  );
}
