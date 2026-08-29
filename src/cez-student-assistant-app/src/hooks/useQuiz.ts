import { useContext } from "react";
import { useTranslation } from "react-i18next";
import { QuizContext } from "../contexts/QuizContext";
import { quizService, UnauthorizedError } from "../services";
import { useAuth } from "./useAuth";
import { useUI } from "./useUI";

export function useQuiz() {
  const ctx = useContext(QuizContext);
  if (!ctx) {
    throw new Error("useQuiz must be used within a QuizProvider");
  }

  const { t } = useTranslation();
  const { handleLogout } = useAuth();
  const { setError } = useUI();



  const refreshQuizzes = async () => {
    try {
      const quizList = await quizService.getQuizzes();
      ctx.setQuizzes(quizList);
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        throw err;
      }
      setError(t("common.errorConnection"));
    }
  };

  return {
    quizzes: ctx.quizzes,
    setQuizzes: ctx.setQuizzes,
    refreshQuizzes,
  };
}
