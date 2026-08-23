import { useContext, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { QuizContext } from "../contexts/QuizContext";
import { quizService, signalRService, UnauthorizedError } from "../services";
import { useAuth } from "./useAuth";
import { useUI } from "./useUI";

export function useQuiz() {
  const ctx = useContext(QuizContext);
  if (!ctx) {
    throw new Error("useQuiz must be used within a QuizProvider");
  }

  const { t } = useTranslation();
  const { isAuthenticated, isAuthChecking, handleLogout } = useAuth();
  const { setError } = useUI();

  const fetchQuizzes = async () => {
    try {
      const quizList = await quizService.getQuizzes();
      ctx.setQuizzes(quizList);
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
      }
    }
  };

  useEffect(() => {
    if (isAuthenticated && !isAuthChecking) {
      fetchQuizzes();
      signalRService.startConnection();

      const unsubscribe = signalRService.subscribeJobStatus((_jobId, _status) => {
        fetchQuizzes();
      });

      return () => {
        unsubscribe();
      };
    } else if (!isAuthenticated) {
      ctx.setQuizzes([]);
      signalRService.stopConnection();
    }
  }, [isAuthenticated, isAuthChecking]);

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
