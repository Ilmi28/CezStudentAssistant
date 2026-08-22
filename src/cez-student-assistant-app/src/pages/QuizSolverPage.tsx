import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { CheckCircle, AlertCircle, Check, ArrowRight, Trophy, RefreshCw } from "lucide-react";
import { quizService, QuestionDifficulty, type QuizAttemptDetailsDto, type QuestionDto } from "../services";

interface QuizSolverPageProps {
  setError: (msg: string) => void;
}

export default function QuizSolverPage({
  setError
}: QuizSolverPageProps) {
  const { attemptId, id } = useParams<{ attemptId?: string; id?: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const [attemptDetails, setAttemptDetails] = useState<QuizAttemptDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);

  const [quizAttempt, setQuizAttempt] = useState<{
    currentQuestionIndex: number;
    selectedOptionIds: string[];
    score: number;
    checked: boolean;
    answers: {
      questionId: string;
      isCorrect: boolean;
      chosenOptionIds: string[];
    }[];
    finished: boolean;
  } | null>(null);

  useEffect(() => {
    loadAttempt();
  }, [attemptId, id]);

  const loadAttempt = async () => {
    const targetAttemptId = attemptId || (location.state as { initialAttempt?: QuizAttemptDetailsDto })?.initialAttempt?.attemptId;
    const targetQuizId = id;

    setLoading(true);
    try {
      let details: QuizAttemptDetailsDto;

      if ((location.state as { initialAttempt?: QuizAttemptDetailsDto })?.initialAttempt) {
        details = (location.state as { initialAttempt: QuizAttemptDetailsDto }).initialAttempt;
      } else if (targetAttemptId) {
        details = await quizService.getQuizAttempt(targetAttemptId);
      } else if (targetQuizId) {
        details = await quizService.startQuiz(targetQuizId);
      } else {
        navigate("/quizzes");
        return;
      }

      setAttemptDetails(details);

      // Restore previously answered questions if any
      const existingAnswers = details.answers || [];
      const restoredAnswers = existingAnswers.map(ans => {
        const q = details.questions.find(item => item.id === ans.questionId);
        const correctOptions = q?.options.filter(o => o.isCorrect).map(o => o.id) || [];
        const isCorrect = ans.selectedOptionIds.length === correctOptions.length &&
          ans.selectedOptionIds.every(optId => correctOptions.includes(optId));
        return {
          questionId: ans.questionId,
          isCorrect,
          chosenOptionIds: ans.selectedOptionIds
        };
      });

      const initialScore = existingAnswers.reduce((sum, ans) => {
        const question = details.questions.find(q => q.id === ans.questionId);
        if (!question) return sum;
        const correctOptions = question.options.filter(o => o.isCorrect).map(o => o.id);
        const isCorrect = ans.selectedOptionIds.length === correctOptions.length &&
          ans.selectedOptionIds.every(optId => correctOptions.includes(optId));
        return isCorrect ? sum + Number(question.points) : sum;
      }, 0);
      const firstUnansweredIndex = details.questions.findIndex(
        q => !existingAnswers.some(ans => ans.questionId === q.id)
      );

      const startIndex = firstUnansweredIndex !== -1 ? firstUnansweredIndex : (existingAnswers.length >= details.questions.length ? 0 : existingAnswers.length);
      const isAlreadyFinished = !details.isPending || (details.questions.length > 0 && existingAnswers.length === details.questions.length);

      setQuizAttempt({
        currentQuestionIndex: startIndex,
        selectedOptionIds: [],
        score: initialScore,
        checked: false,
        answers: restoredAnswers,
        finished: isAlreadyFinished
      });
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to load quiz attempt:", err);
      setError(t("quizSolver.loadingQuiz"));
      navigate("/quizzes");
    } finally {
      setLoading(false);
    }
  };

  const getDifficultyBadge = (difficulty?: QuestionDifficulty) => {
    switch (difficulty) {
      case QuestionDifficulty.Easy:
        return (
          <span className="text-[10px] bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 font-bold px-2 py-0.5 rounded uppercase">
            {t("quizSolver.difficulty.easy")}
          </span>
        );
      case QuestionDifficulty.Hard:
        return (
          <span className="text-[10px] bg-rose-500/15 text-rose-600 dark:text-rose-400 font-bold px-2 py-0.5 rounded uppercase">
            {t("quizSolver.difficulty.hard")}
          </span>
        );
      case QuestionDifficulty.Medium:
      default:
        return (
          <span className="text-[10px] bg-amber-500/15 text-amber-600 dark:text-amber-400 font-bold px-2 py-0.5 rounded uppercase">
            {t("quizSolver.difficulty.medium")}
          </span>
        );
    }
  };

  const handleToggleOption = (optionId: string, isMultipleChoice: boolean) => {
    if (!quizAttempt || quizAttempt.checked) return;
    setQuizAttempt(prev => {
      if (!prev) return null;
      if (isMultipleChoice) {
        const selected = prev.selectedOptionIds.includes(optionId)
          ? prev.selectedOptionIds.filter(x => x !== optionId)
          : [...prev.selectedOptionIds, optionId];
        return { ...prev, selectedOptionIds: selected };
      } else {
        return { ...prev, selectedOptionIds: [optionId] };
      }
    });
  };

  const handleCheckAnswer = async () => {
    if (!quizAttempt || !attemptDetails) return;
    const currentQuestion = attemptDetails.questions[quizAttempt.currentQuestionIndex];
    
    const correctOptions = currentQuestion.options.filter(o => o.isCorrect);
    const correctOptionIds = correctOptions.map(o => o.id);
    
    const isCorrect = 
      quizAttempt.selectedOptionIds.length === correctOptionIds.length &&
      quizAttempt.selectedOptionIds.every(x => correctOptionIds.includes(x));
    
    const earnedPoints = isCorrect ? currentQuestion.points : 0;

    try {
      await quizService.submitAnswer(
        attemptDetails.attemptId,
        currentQuestion.id,
        quizAttempt.selectedOptionIds
      );
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to submit answer:", err);
    }

    setQuizAttempt(prev => {
      if (!prev) return null;
      return {
        ...prev,
        checked: true,
        score: prev.score + Number(earnedPoints),
        answers: [
          ...prev.answers,
          {
            questionId: currentQuestion.id,
            isCorrect,
            chosenOptionIds: prev.selectedOptionIds
          }
        ]
      };
    });
  };

  const handleNextQuestion = async () => {
    if (!quizAttempt || !attemptDetails) return;
    const isLast = quizAttempt.currentQuestionIndex >= attemptDetails.questions.length - 1;
    if (isLast) {
      try {
        await quizService.completeQuizAttempt(attemptDetails.attemptId);
      } catch (err) {
        console.warn("[QuizSolverPage] Failed to complete quiz attempt:", err);
      }
      setQuizAttempt(prev => (prev ? { ...prev, finished: true } : null));
    } else {
      setQuizAttempt(prev => {
        if (!prev) return null;
        return {
          ...prev,
          currentQuestionIndex: prev.currentQuestionIndex + 1,
          selectedOptionIds: [],
          checked: false
        };
      });
    }
  };

  const handleRetry = async () => {
    if (!attemptDetails) return;
    try {
      const newAttempt = await quizService.startQuiz(attemptDetails.quizId);
      setAttemptDetails(newAttempt);
      setQuizAttempt({
        currentQuestionIndex: 0,
        selectedOptionIds: [],
        score: 0,
        checked: false,
        answers: [],
        finished: false
      });
      navigate(`/quiz/attempt/${newAttempt.attemptId}`, { replace: true });
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to retry quiz:", err);
      navigate(`/quiz/${attemptDetails.quizId}`);
    }
  };

  const handleCancel = () => {
    if (attemptDetails?.quizId) {
      navigate(`/quiz/${attemptDetails.quizId}`);
    } else {
      navigate("/quizzes");
    }
  };

  if (loading || !attemptDetails || !quizAttempt) {
    return (
      <div className="flex justify-center py-12">
        <div className="flex items-center gap-3 bg-card px-6 py-4 rounded-lg border border-border shadow-sm">
          <RefreshCw size={18} className="animate-spin text-primary" />
          <span className="text-[13px] font-medium text-muted-foreground">{t("quizSolver.loadingQuiz")}</span>
        </div>
      </div>
    );
  }

  const { currentQuestionIndex: qIndex, selectedOptionIds, checked, score, finished, answers } = quizAttempt;
  const qCount = attemptDetails.questions.length;
  
  if (finished) {
    const totalPointsMax = attemptDetails.questions.reduce((sum, q) => sum + Number(q.points), 0);
    return (
      <div className="max-w-2xl mx-auto bg-card rounded-lg border border-border p-8 text-center shadow-sm space-y-6 animate-in fade-in duration-300">
        <div className="inline-flex w-16 h-16 rounded-full bg-primary/10 items-center justify-center text-primary mb-2 border-2 border-primary/25">
          <Trophy size={32} />
        </div>
        <div className="space-y-2">
          <h2 className="text-xl font-bold text-foreground">{t("quizSolver.finishedTitle")}</h2>
          <p className="text-sm text-muted-foreground">{t("quizSolver.finishedDesc")}</p>
        </div>

        {/* Score metrics */}
        <div className="max-w-xs mx-auto bg-muted rounded-lg p-5 border border-border space-y-1">
          <div className="text-[10px] uppercase font-bold text-muted-foreground tracking-wider">{t("quizSolver.finishedScoreLabel")}</div>
          <div className="text-4xl font-bold text-primary">
            {score.toFixed(1)} pkt
          </div>
          <div className="text-xs text-muted-foreground">
            {t("quizSolver.finishedMaxLabel", { max: totalPointsMax })}
          </div>
        </div>

        {/* Individual answers recap */}
        <div className="text-left space-y-3 pt-4 border-t border-border">
          <h4 className="text-[11px] uppercase font-bold text-muted-foreground tracking-wider mb-2">{t("quizSolver.finishedSummary")}</h4>
          {attemptDetails.questions.map((q, idx) => {
            const isCorrect = answers[idx]?.isCorrect;
            return (
              <div key={q.id} className="flex items-start justify-between text-[13px] py-1 border-b border-border last:border-b-0">
                <span className="text-muted-foreground font-medium truncate max-w-[360px]">{idx + 1}. {q.content}</span>
                <div className="flex items-center gap-2 shrink-0 ml-4">
                  {getDifficultyBadge(q.difficulty)}
                  <span className={`font-semibold uppercase text-[10px] ${isCorrect ? "text-green-600 dark:text-green-400" : "text-red-500 dark:text-red-400"}`}>
                    {isCorrect ? t("quizSolver.finishedCorrectFeedback") : t("quizSolver.finishedIncorrectFeedback")}
                  </span>
                </div>
              </div>
            );
          })}
        </div>

        <div className="pt-6 flex gap-4">
          <button
            onClick={handleRetry}
            className="flex-1 bg-card hover:bg-muted text-primary border border-primary py-2.5 rounded text-[13px] font-medium transition-colors cursor-pointer"
          >
            {t("quizSolver.finishedRetryBtn")}
          </button>
          <button
            onClick={handleCancel}
            className="flex-1 bg-primary hover:bg-primary/90 text-white py-2.5 rounded text-[13px] font-medium transition-colors cursor-pointer shadow-sm"
          >
            {t("quizSolver.finishedBackBtn")}
          </button>
        </div>
      </div>
    );
  }

  const question: QuestionDto = attemptDetails.questions[qIndex];
  const isMultiple = question.type === 1;

  return (
    <div className="max-w-2xl mx-auto space-y-6 animate-in fade-in duration-300">
      {/* Header info */}
      <div className="flex items-center justify-between">
        <div>
          <span className="text-[10px] uppercase font-bold text-muted-foreground tracking-wider">
            {t("quizSolver.solvingQuiz", { course: attemptDetails.courseName })}
          </span>
          <h2 className="text-base font-bold text-foreground mt-0.5 line-clamp-1">
            {attemptDetails.displayName}
          </h2>
        </div>
        <button
          onClick={() => {
            if (confirm(t("quizSolver.cancelConfirm"))) {
              handleCancel();
            }
          }}
          className="text-[12px] text-[#c44444] hover:underline font-semibold cursor-pointer"
        >
          {t("quizSolver.cancelBtn")}
        </button>
      </div>

      <div className="bg-card rounded-lg border border-border shadow-sm overflow-hidden">
        {/* Question progress header */}
        <div className="bg-muted px-6 py-3 border-b border-border flex justify-between items-center">
          <span className="text-xs text-muted-foreground">
            {t("quizSolver.questionProgress", { current: qIndex + 1, total: qCount })}
          </span>
          <div className="flex items-center gap-2">
            {getDifficultyBadge(question.difficulty)}
            <span className="text-[10px] bg-primary/15 text-primary font-bold px-2 py-0.5 rounded uppercase">
              {t("quizSolver.points", { points: question.points })}
            </span>
          </div>
        </div>

        {/* Progress bar */}
        <div className="h-1 bg-muted">
          <div
            className="h-full bg-primary transition-all duration-300"
            style={{ width: `${((qIndex + 1) / qCount) * 100}%` }}
          />
        </div>

        <div className="p-6 md:p-8 space-y-6">
          {/* Question text */}
          <h3 className="text-base font-bold text-foreground leading-relaxed">
            {question.content}
          </h3>

          {/* Options */}
          <div className="space-y-2">
            {question.options.map((opt) => {
              const isSelected = selectedOptionIds.includes(opt.id);
              
              let optionStyle = "border-border hover:bg-muted/30 hover:border-primary/30 text-foreground";
              if (isSelected) {
                optionStyle = "bg-primary/5 border-primary text-foreground";
              }
              
              if (checked) {
                if (opt.isCorrect) {
                  optionStyle = "bg-green-100 dark:bg-green-950/20 border-green-600 dark:border-green-500 text-green-900 dark:text-green-300 font-medium";
                } else if (isSelected && !opt.isCorrect) {
                  optionStyle = "bg-red-100 dark:bg-red-950/20 border-red-500 dark:border-red-500 text-red-900 dark:text-red-300";
                } else {
                  optionStyle = "border-border opacity-50 text-muted-foreground";
                }
              }

              return (
                <button
                  key={opt.id}
                  onClick={() => handleToggleOption(opt.id, isMultiple)}
                  disabled={checked}
                  className={`w-full p-4 rounded text-left text-[13px] border transition-all flex items-start gap-3 cursor-pointer ${optionStyle}`}
                >
                  <div className="mt-0.5 flex-shrink-0">
                    <div className={`w-4.5 h-4.5 rounded-sm border flex items-center justify-center ${
                      isSelected 
                        ? "bg-primary border-primary text-white" 
                        : "border-border"
                    }`}>
                      {isSelected && <Check size={11} strokeWidth={3} />}
                    </div>
                  </div>
                  <span className="flex-1">{opt.content}</span>
                  {checked && opt.isCorrect && (
                    <span className="text-[10px] text-green-700 dark:text-green-400 font-bold uppercase shrink-0">{t("quizSolver.correctOption")}</span>
                  )}
                  {checked && isSelected && !opt.isCorrect && (
                    <span className="text-[10px] text-red-600 dark:text-red-400 font-bold uppercase shrink-0">{t("quizSolver.incorrectOption")}</span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Feedback messages after check */}
          {checked && (
            <div className={`p-4 rounded border text-[13px] flex items-start gap-3 ${
              answers[qIndex]?.isCorrect 
                ? "bg-green-100/50 dark:bg-green-950/10 border-green-200 dark:border-green-800/40 text-green-900 dark:text-green-300" 
                : "bg-red-100/50 dark:bg-red-950/10 border-red-200 dark:border-red-800/40 text-red-900 dark:text-red-300"
            }`}>
              <div className="mt-0.5">
                {answers[qIndex]?.isCorrect ? <CheckCircle size={15} /> : <AlertCircle size={15} />}
              </div>
              <div>
                <span className="font-bold">{answers[qIndex]?.isCorrect ? t("quizSolver.feedbackCorrect") : t("quizSolver.feedbackIncorrect")}</span>
                {" "}{t("quizSolver.feedbackEarned", { earned: answers[qIndex]?.isCorrect ? question.points : 0, total: question.points })}
              </div>
            </div>
          )}

          {/* Actions block */}
          <div className="pt-4 border-t border-border flex justify-end">
            {!checked ? (
              <button
                onClick={handleCheckAnswer}
                disabled={selectedOptionIds.length === 0}
                className="bg-primary hover:bg-primary/90 disabled:bg-[#888] text-white px-5 py-2.5 rounded text-[13px] font-medium transition-colors flex items-center justify-center gap-1.5 cursor-pointer shadow-sm"
              >
                {t("quizSolver.checkBtn")}
              </button>
            ) : (
              <button
                onClick={handleNextQuestion}
                className="bg-primary hover:bg-primary/90 text-white px-5 py-2.5 rounded text-[13px] font-medium transition-all cursor-pointer shadow-sm inline-flex items-center gap-1.5"
              >
                {qIndex >= qCount - 1 ? t("quizSolver.finishBtn") : t("quizSolver.nextBtn")}
                <ArrowRight size={13} />
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
