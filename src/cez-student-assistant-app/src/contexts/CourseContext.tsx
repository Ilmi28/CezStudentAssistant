import { createContext, useState, type ReactNode } from "react";
import type { CourseDto } from "../services";

export interface CourseContextState {
  courses: CourseDto[];
  setCourses: React.Dispatch<React.SetStateAction<CourseDto[]>>;
}

export const CourseContext = createContext<CourseContextState | null>(null);

export function CourseProvider({ children }: { children: ReactNode }) {
  const [courses, setCourses] = useState<CourseDto[]>([]);

  return (
    <CourseContext.Provider value={{ courses, setCourses }}>
      {children}
    </CourseContext.Provider>
  );
}
