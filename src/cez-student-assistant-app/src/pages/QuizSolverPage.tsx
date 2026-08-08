import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { CheckCircle, AlertCircle, Check, ArrowRight, Trophy, RefreshCw } from "lucide-react";
import { api, type QuizDetailsDto, type QuestionDto } from "../services/api";

interface QuizSolverPageProps {
  setError: (msg: string) => void;
}

export default function QuizSolverPage({
  setError
}: QuizSolverPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [selectedQuiz, setSelectedQuiz] = useState<QuizDetailsDto | null>(null);
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
    loadQuizDetails();
  }, [id]);

  const loadQuizDetails = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const details = await api.getQuizDetails(id);
      setSelectedQuiz(details);
      setQuizAttempt({
        currentQuestionIndex: 0,
        selectedOptionIds: [],
        score: 0,
        checked: false,
        answers: [],
        finished: false
      });
    } catch {
      setError(t("quizSolver.loadingQuiz"));
      navigate("/quizzes");
    } finally {
      setLoading(false);
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

  const handleCheckAnswer = () => {
    if (!quizAttempt || !selectedQuiz) return;
    const currentQuestion = selectedQuiz.questions[quizAttempt.currentQuestionIndex];
    
    const correctOptions = currentQuestion.options.filter(o => o.isCorrect);
    const correctOptionIds = correctOptions.map(o => o.id);
    
    const isCorrect = 
      quizAttempt.selectedOptionIds.length === correctOptionIds.length &&
      quizAttempt.selectedOptionIds.every(x => correctOptionIds.includes(x));
    
    const earnedPoints = isCorrect ? currentQuestion.points : 0;

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

  const handleNextQuestion = () => {
    if (!quizAttempt || !selectedQuiz) return;
    const isLast = quizAttempt.currentQuestionIndex >= selectedQuiz.questions.length - 1;
    setQuizAttempt(prev => {
      if (!prev) return null;
      if (isLast) {
        return { ...prev, finished: true };
      } else {
        return {
          ...prev,
          currentQuestionIndex: prev.currentQuestionIndex + 1,
          selectedOptionIds: [],
          checked: false
        };
      }
    });
  };

  const handleRetry = () => {
    loadQuizDetails();
  };

  const handleCancel = () => {
    navigate("/quizzes");
  };

  if (loading || !selectedQuiz || !quizAttempt) {
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
  const qCount = selectedQuiz.questions.length;
  
  if (finished) {
    const totalPointsMax = selectedQuiz.questions.reduce((sum, q) => sum + q.points, 0);
    return (
      <div className="max-w-2xl mx-auto bg-card rounded-lg border border-border p-8 text-center shadow-sm space-y-6 animate-in fade-in duration-300">
        <div className="inline-flex w-16 h-16 rounded-full bg-primary/10 items-center justify-center text-primary mb-2 border-2 border-primary/25">
          <Trophy size={32} />
        </div>
        <div className="space-y-2">
          <h2 style={{ fontFamily: "Roboto Slab, serif" }} className="text-xl font-bold text-foreground">{t("quizSolver.finishedTitle")}</h2>
          <p className="text-[13px] text-muted-foreground">{t("quizSolver.finishedDesc")}</p>
        </div>

        {/* Score metrics */}
        <div className="max-w-xs mx-auto bg-muted rounded-lg p-5 border border-border space-y-1">
          <div className="text-[10px] uppercase font-bold text-muted-foreground tracking-wider">{t("quizSolver.finishedScoreLabel")}</div>
          <div className="text-[36px] font-bold text-primary" style={{ fontFamily: "Roboto Slab, serif" }}>
            {score.toFixed(1)} pkt
          </div>
          <div className="text-[12px] text-muted-foreground font-mono">
            {t("quizSolver.finishedMaxLabel", { max: totalPointsMax })}
          </div>
        </div>

        {/* Individual answers recap */}
        <div className="text-left space-y-3 pt-4 border-t border-border">
          <h4 className="text-[11px] uppercase font-bold text-muted-foreground tracking-wider mb-2">{t("quizSolver.finishedSummary")}</h4>
          {selectedQuiz.questions.map((q, idx) => {
            const isCorrect = answers[idx]?.isCorrect;
            return (
              <div key={q.id} className="flex items-start justify-between text-[13px] py-1 border-b border-border last:border-b-0">
                <span className="text-muted-foreground font-medium truncate max-w-[420px]">{idx + 1}. {q.content}</span>
                <span className={`font-semibold shrink-0 ml-4 uppercase text-[10px] ${isCorrect ? "text-green-600 dark:text-green-400" : "text-red-500 dark:text-red-400"}`}>
                  {isCorrect ? t("quizSolver.finishedCorrectFeedback") : t("quizSolver.finishedIncorrectFeedback")}
                </span>
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

  const question: QuestionDto = selectedQuiz.questions[qIndex];
  const isMultiple = question.type === 1;

  return (
    <div className="max-w-2xl mx-auto space-y-6 animate-in fade-in duration-300">
      {/* Header info */}
      <div className="flex items-center justify-between">
        <div>
          <span className="text-[10px] uppercase font-bold text-muted-foreground tracking-wider">
            {t("quizSolver.solvingQuiz", { course: selectedQuiz.courseName })}
          </span>
          <h2 style={{ fontFamily: "Roboto Slab, serif" }} className="text-base font-bold text-foreground mt-0.5 line-clamp-1">
            {selectedQuiz.displayName || selectedQuiz.name}
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
          <span className="text-[11px] font-mono text-muted-foreground">
            {t("quizSolver.questionProgress", { current: qIndex + 1, total: qCount })}
          </span>
          <span className="text-[9px] bg-primary/15 text-primary font-bold px-2 py-0.5 rounded font-mono uppercase">
            {t("quizSolver.points", { points: question.points })}
          </span>
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
          <h3 style={{ fontFamily: "Roboto Slab, serif" }} className="text-base font-bold text-foreground leading-relaxed">
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
