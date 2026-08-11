import { authService } from "./authService";
import { courseService } from "./courseService";
import { quizService } from "./quizService";
import { userService } from "./userService";
import { cezService } from "./cezService";

export * from "./baseClient";
export * from "./authService";
export * from "./courseService";
export * from "./quizService";
export * from "./userService";
export * from "./cezService";

export const api = {
  ...authService,
  ...courseService,
  ...quizService,
  ...userService,
  ...cezService,
};
