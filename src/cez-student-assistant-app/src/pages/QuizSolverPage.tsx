import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Check, ChevronLeft, ChevronRight, Clock, X } from "lucide-react";
import { quizService, QuizAttemptStatus, QuestionDifficulty, QuestionType, type QuizAttemptDetailsDto, type QuestionDto } from "../services";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import ConfirmModal from "../components/ConfirmModal";
import LoadingScreen from "../components/LoadingScreen";
import { useCountdown, useQuiz } from "../hooks";
import { getScoreBadgeColorClass } from "../utils/scoreUtils";

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
  const { refreshQuizzes } = useQuiz();

  const [attemptDetails, setAttemptDetails] = useState<QuizAttemptDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [isReviewMode, setIsReviewMode] = useState(false);
  const [selectedAnswers, setSelectedAnswers] = useState<Record<string, string[]>>({});
  const [finished, setFinished] = useState(false);
  const [finalScore, setFinalScore] = useState(0);
  const [showSubmitModal, setShowSubmitModal] = useState(false);

  const {
    formatted: timeLeftFormatted,
    isExpired,
    isTimeLow,
  } = useCountdown(
    attemptDetails?.expiresAt && !finished ? attemptDetails.expiresAt : null,
    () => {
      if (!finished && attemptDetails) {
        handleConfirmSubmit();
      }
    }
  );

  const getDifficultyPoints = (diff?: QuestionDifficulty) => {
    switch (diff) {
      case QuestionDifficulty.Easy: return 1;
      case QuestionDifficulty.Hard: return 3;
      case QuestionDifficulty.Medium:
      default: return 2;
    }
  };

  const calculateQuestionPoints = (q: QuestionDto, selectedIds: string[] = []) => {
    const maxPoints = getDifficultyPoints(q.difficulty);
    const correctOptionIds = q.options.filter(o => o.isCorrect).map(o => o.id);
    if (correctOptionIds.length === 0) return 0;

    if (q.type === QuestionType.SingleChoice) {
      const isSingleCorrect = selectedIds.length === 1 && correctOptionIds.includes(selectedIds[0]);
      return isSingleCorrect ? maxPoints : 0;
    }

    const correctSelectedCount = selectedIds.filter(id => correctOptionIds.includes(id)).length;
    const incorrectSelectedCount = selectedIds.filter(id => !correctOptionIds.includes(id)).length;

    const netCorrect = correctSelectedCount - incorrectSelectedCount;
    if (netCorrect <= 0) return 0;

    const fraction = netCorrect / correctOptionIds.length;
    return parseFloat((maxPoints * fraction).toFixed(2));
  };

  useEffect(() => {
    loadAttempt();
  }, [attemptId, id]);

  const loadAttempt = async () => {
    setLoading(true);
    try {
      let details: QuizAttemptDetailsDto;

      const stateAttempt = (location.state as { initialAttempt?: QuizAttemptDetailsDto })?.initialAttempt;
      if (stateAttempt && stateAttempt.attemptId === attemptId) {
        details = stateAttempt;
      } else if (attemptId) {
        details = await quizService.getQuizAttempt(attemptId);
      } else if (id) {
        details = await quizService.startQuiz(id);
      } else {
        navigate("/quizzes");
        return;
      }

      setAttemptDetails(details);

      const initialAnswers: Record<string, string[]> = {};
      if (details.answers && details.answers.length > 0) {
        details.answers.forEach(a => {
          initialAnswers[a.questionId] = a.selectedOptionIds;
        });
      }
      setSelectedAnswers(initialAnswers);

      if (details.status === QuizAttemptStatus.Completed) {
        const totalScore = details.questions.reduce((sum, q) => {
          const userSelected = initialAnswers[q.id] || [];
          return sum + calculateQuestionPoints(q, userSelected);
        }, 0);

        setFinalScore(parseFloat(totalScore.toFixed(2)));
        setFinished(true);
      } else {
        const firstUnansweredIndex = details.questions.findIndex(
          q => !initialAnswers[q.id] || initialAnswers[q.id].length === 0
        );
        setCurrentQuestionIndex(firstUnansweredIndex !== -1 ? firstUnansweredIndex : 0);
        setIsReviewMode(false);
        setFinished(false);
      }
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to load quiz attempt:", err);
      setError(t("common.genericError"));
      navigate("/quizzes");
    } finally {
      setLoading(false);
    }
  };

  const getTypeBadge = (type?: QuestionType) => {
    const isMultiple = type === QuestionType.MultipleChoice;
    return (
      <span className="text-[10px] bg-muted text-muted-foreground font-semibold px-2 py-0.5 rounded border border-border">
        {isMultiple ? t("quizSolver.type.multiple") : t("quizSolver.type.single")}
      </span>
    );
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

  const handleToggleOption = async (questionId: string, optionId: string, isMultipleChoice: boolean) => {
    if (finished || !attemptDetails) return;

    const currentSelected = selectedAnswers[questionId] || [];
    let updatedSelected: string[];

    if (isMultipleChoice) {
      updatedSelected = currentSelected.includes(optionId)
        ? currentSelected.filter(id => id !== optionId)
        : [...currentSelected, optionId];
    } else {
      updatedSelected = [optionId];
    }

    setSelectedAnswers(prev => ({
      ...prev,
      [questionId]: updatedSelected
    }));

    if (updatedSelected.length > 0) {
      try {
        await quizService.submitAnswer(
          attemptDetails.attemptId,
          questionId,
          updatedSelected
        );
      } catch (err) {
        console.warn("[QuizSolverPage] Failed to save answer:", err);
      }
    }
  };

  const handleConfirmSubmit = async () => {
    if (!attemptDetails) return;

    try {
      await quizService.completeQuizAttempt(attemptDetails.attemptId);
      refreshQuizzes().catch(() => {});
      const updatedDetails = await quizService.getQuizAttempt(attemptDetails.attemptId);
      setAttemptDetails(updatedDetails);

      const totalScore = updatedDetails.questions.reduce((sum, q) => {
        const userSelected = selectedAnswers[q.id] || [];
        return sum + calculateQuestionPoints(q, userSelected);
      }, 0);

      setFinalScore(parseFloat(totalScore.toFixed(2)));
      setFinished(true);
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to complete quiz attempt:", err);
      setError(t("common.genericError"));
    }
  };

  const handleCancel = () => {
    if (attemptDetails?.quizId) {
      navigate(`/quiz/${attemptDetails.quizId}`, {
        state: { fromPath: location.state?.fromPath }
      });
    } else if (location.state?.fromPath) {
      navigate(location.state.fromPath);
    } else {
      navigate("/quizzes");
    }
  };

  if (loading || !attemptDetails) {
    return <LoadingScreen message={t("quizSolver.loadingQuiz")} />;
  }

  const qCount = attemptDetails.questions.length;

  if (finished) {
    const totalPointsMax = attemptDetails.questions.reduce((sum, q) => sum + getDifficultyPoints(q.difficulty), 0);
    const formatScore = (val: number) => {
      if (Number.isInteger(val)) return val.toString();
      return parseFloat(val.toFixed(2)).toString();
    };

    return (
      <div className="max-w-4xl mx-auto space-y-5 animate-in fade-in duration-300">
        {/* Top Header */}
        <div className="flex items-center gap-3.5 min-w-0">
          <button
            type="button"
            onClick={handleCancel}
            title={t("quizDetails.backBtn")}
            aria-label={t("quizDetails.backBtn")}
            className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors shadow-xs cursor-pointer shrink-0"
          >
            <ChevronLeft size={22} strokeWidth={2.25} className="shrink-0" />
          </button>
          <div className="flex flex-col justify-center min-w-0">
            <h1 className="text-xl font-bold text-foreground truncate">
              {attemptDetails.name}
            </h1>
          </div>
        </div>

        <div className="flex flex-col md:flex-row gap-4 items-start">
          {/* Main Report Card */}
          <div className="flex-1 w-full bg-card rounded-xl border border-border shadow-sm overflow-hidden p-6 md:p-8 space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-3 pb-4 border-b border-border">
              <div className="flex items-center gap-3">
                <span className="text-base font-bold text-foreground">
                  {formatScore(finalScore)} / {formatScore(totalPointsMax)} pkt
                </span>
                {(() => {
                  const scorePercent = totalPointsMax > 0 ? Math.round((finalScore / totalPointsMax) * 100) : 0;
                  return (
                    <span className={`text-xs font-bold px-2.5 py-0.5 rounded-md border ${getScoreBadgeColorClass(scorePercent)}`}>
                      {scorePercent}%
                    </span>
                  );
                })()}
              </div>

              <div className="flex items-center gap-3 text-xs font-medium text-muted-foreground">
                {(() => {
                  if (!attemptDetails?.startedAt) return null;
                  const start = new Date(attemptDetails.startedAt).getTime();
                  const end = attemptDetails.completedAt
                    ? new Date(attemptDetails.completedAt).getTime()
                    : Date.now();

                  const diffMs = Math.max(0, end - start);
                  const totalSec = Math.floor(diffMs / 1000);
                  const mins = Math.floor(totalSec / 60);
                  const secs = totalSec % 60;
                  const formattedSpent = mins === 0 ? `${secs} s` : `${mins} min ${secs} s`;

                  return (
                    <div className="flex items-center gap-1.5 bg-background px-2.5 py-1 rounded-lg border border-border/60">
                      <Clock size={13} className="text-primary shrink-0" />
                      <span>
                        {t("quizSolver.timeSpent", "Czas")}: <strong className="text-foreground font-semibold">{formattedSpent}</strong>
                      </span>
                    </div>
                  );
                })()}

                {attemptDetails.timeLimitMinutes && (
                  <div className="flex items-center gap-1.5 bg-background px-2.5 py-1 rounded-lg border border-border/60">
                    <span>
                      {t("quizSolver.timeLimit", "Limit")}: <strong className="text-foreground font-semibold">{attemptDetails.timeLimitMinutes} min</strong>
                    </span>
                  </div>
                )}
              </div>
            </div>

            {/* Questions list with feedback and selected options */}
            <div className="space-y-5">
              {attemptDetails.questions.map((q, idx) => {
                const userSelected = selectedAnswers[q.id] || [];
                const earnedPoints = calculateQuestionPoints(q, userSelected);
                const maxPoints = getDifficultyPoints(q.difficulty);
                const isFull = earnedPoints === maxPoints;
                const isPartial = earnedPoints > 0 && earnedPoints < maxPoints;
                const isMultiple = q.type === QuestionType.MultipleChoice;

                return (
                  <div
                    key={q.id}
                    id={`report-question-${idx}`}
                    className={`p-4 rounded-xl bg-background/50 border transition-colors space-y-3 ${
                      isFull
                        ? "border-border hover:border-emerald-500/30"
                        : isPartial
                          ? "border-border hover:border-amber-500/30"
                          : "border-border hover:border-rose-500/30"
                    }`}
                  >
                    <div className="space-y-1.5">
                      <div className="flex items-center justify-between gap-2">
                        <div className="flex items-center gap-2">
                          {getTypeBadge(q.type)}
                          {getDifficultyBadge(q.difficulty)}
                        </div>
                        <span
                          className={`text-[11px] font-bold px-2.5 py-0.5 rounded-md border ${
                            isFull
                              ? "bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 border-emerald-500/30"
                              : isPartial
                                ? "bg-amber-500/15 text-amber-600 dark:text-amber-400 border-amber-500/30"
                                : "bg-rose-500/15 text-rose-600 dark:text-rose-400 border-rose-500/30"
                          }`}
                        >
                          {formatScore(earnedPoints)} / {formatScore(maxPoints)} pkt
                        </span>
                      </div>

                      <h4 className="text-[13px] font-semibold text-foreground leading-snug">
                        {idx + 1}. {q.content}
                      </h4>
                    </div>

                    <div className="space-y-1.5 pt-1">
                      {q.options.map((opt) => {
                        const isSelected = userSelected.includes(opt.id);
                        const isCorrectOption = opt.isCorrect;

                        let optionStyle = "border-border/60 text-muted-foreground bg-card/40 opacity-70";
                        let indicatorStyle = "border-border bg-card";

                        if (isSelected && isCorrectOption) {
                          optionStyle = "bg-emerald-500/10 border-emerald-500/40 text-foreground font-medium";
                          indicatorStyle = "bg-emerald-600 border-emerald-600 text-white";
                        } else if (isSelected && !isCorrectOption) {
                          optionStyle = "bg-rose-500/10 border-rose-500/40 text-foreground font-medium";
                          indicatorStyle = "bg-rose-600 border-rose-600 text-white";
                        } else if (!isSelected && isCorrectOption) {
                          optionStyle = "border-emerald-500/30 bg-emerald-500/5 text-foreground";
                          indicatorStyle = "border-emerald-500/60 text-emerald-600";
                        }

                        return (
                          <div
                            key={opt.id}
                            className={`p-2.5 rounded-lg text-xs border flex items-center gap-2.5 transition-colors ${optionStyle}`}
                          >
                            <div className={`w-3.5 h-3.5 ${isMultiple ? "rounded-xs" : "rounded-full"} border flex items-center justify-center shrink-0 ${indicatorStyle}`}>
                              {isSelected && isCorrectOption && <Check size={9} strokeWidth={3} />}
                              {isSelected && !isCorrectOption && <X size={9} strokeWidth={3} />}
                              {!isSelected && isCorrectOption && <Check size={9} strokeWidth={3} />}
                            </div>
                            <span className="leading-relaxed">{opt.content}</span>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Right sticky question tiles - green for correct, amber for partial, red for incorrect */}
          <div className="grid grid-cols-5 gap-2 shrink-0 sticky top-6 self-start">
            {attemptDetails.questions.map((q, idx) => {
              const userSelected = selectedAnswers[q.id] || [];
              const earnedPoints = calculateQuestionPoints(q, userSelected);
              const maxPoints = getDifficultyPoints(q.difficulty);
              const isFull = earnedPoints === maxPoints;
              const isPartial = earnedPoints > 0 && earnedPoints < maxPoints;

              const pillStyle = isFull
                ? "bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 border-emerald-500/40 hover:bg-emerald-500/25 font-bold"
                : isPartial
                  ? "bg-amber-500/15 text-amber-600 dark:text-amber-400 border-amber-500/40 hover:bg-amber-500/25 font-bold"
                  : "bg-rose-500/15 text-rose-600 dark:text-rose-400 border-rose-500/40 hover:bg-rose-500/25 font-bold";

              return (
                <button
                  key={q.id}
                  type="button"
                  onClick={() => {
                    const el = document.getElementById(`report-question-${idx}`);
                    if (el) {
                      el.scrollIntoView({ behavior: "smooth", block: "center" });
                    }
                  }}
                  className={`w-9 h-9 rounded-lg border text-xs flex items-center justify-center transition-all cursor-pointer select-none ${pillStyle}`}
                  title={`${idx + 1}. ${isFull ? t("quizSolver.finishedCorrectFeedback") : isPartial ? "Częściowo poprawna" : t("quizSolver.finishedIncorrectFeedback")}`}
                >
                  {idx + 1}
                </button>
              );
            })}
          </div>
        </div>
      </div>
    );
  }

  const question: QuestionDto = attemptDetails.questions[currentQuestionIndex];
  const isMultiple = question.type === QuestionType.MultipleChoice;
  const currentQuestionSelected = selectedAnswers[question.id] || [];

  return (
    <div className="max-w-4xl mx-auto space-y-5 animate-in fade-in duration-300">
      {/* Top Header: Back btn + Title */}
      <div className="flex items-center gap-3.5 min-w-0">
        <button
          type="button"
          onClick={handleCancel}
          title={t("quizDetails.backBtn")}
          aria-label={t("quizDetails.backBtn")}
          className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors shadow-xs cursor-pointer shrink-0"
        >
          <ChevronLeft size={22} strokeWidth={2.25} className="shrink-0" />
        </button>
        <div className="flex flex-col justify-center min-w-0">
          <h1 className="text-xl font-bold text-foreground truncate">
            {attemptDetails.name}
          </h1>
        </div>
      </div>

      <div className="flex flex-col md:flex-row gap-4 items-start">
        {/* Main Content Column */}
        <div className="flex-1 w-full">
          {/* Main Content Area: Question Card or Review Screen */}
          {isReviewMode ? (
            <div className="w-full bg-card rounded-xl border border-border shadow-sm overflow-hidden p-6 md:p-8 space-y-6">
              <h3 className="text-base font-bold text-foreground">
                {t("quizSolver.reviewTitle")}
              </h3>

              {/* Questions list with options and selected answer */}
              <div className="space-y-4">
                {attemptDetails.questions.map((q, idx) => {
                  const selectedOptionIds = selectedAnswers[q.id] || [];
                  const isMultiple = q.type === QuestionType.MultipleChoice;

                  return (
                    <div
                      key={q.id}
                      className="p-4 rounded-xl bg-background/50 border border-border space-y-3"
                    >
                      <div className="space-y-1.5">
                        <div>
                          {getTypeBadge(q.type)}
                        </div>
                        <h4 className="text-[13px] font-semibold text-foreground leading-snug">
                          {idx + 1}. {q.content}
                        </h4>
                      </div>

                      <div className="space-y-1.5 pt-1">
                        {q.options.map((opt) => {
                          const isSelected = selectedOptionIds.includes(opt.id);
                          return (
                            <div
                              key={opt.id}
                              className={`p-2.5 rounded-lg text-xs border flex items-center gap-2.5 transition-colors ${
                                isSelected
                                  ? "bg-primary/10 border-primary/40 text-foreground font-medium"
                                  : "border-border/60 text-muted-foreground bg-card/40"
                              }`}
                            >
                              <div className={`w-3.5 h-3.5 ${isMultiple ? "rounded-xs" : "rounded-full"} border flex items-center justify-center shrink-0 ${
                                isSelected ? "bg-primary border-primary text-white" : "border-border"
                              }`}>
                                {isSelected && <Check size={9} strokeWidth={3} />}
                              </div>
                              <span className="leading-relaxed">{opt.content}</span>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Bottom Actions for Review Screen */}
              <div className="pt-5 border-t border-border flex items-center justify-between gap-3">
                <SecondaryButton
                  size="sm"
                  onClick={() => {
                    setCurrentQuestionIndex(qCount - 1);
                    setIsReviewMode(false);
                  }}
                  icon={<ChevronLeft size={15} />}
                >
                  {t("quizSolver.reviewBackToQuestions")}
                </SecondaryButton>

                <PrimaryButton
                  size="sm"
                  onClick={() => setShowSubmitModal(true)}
                  className="px-6 font-semibold"
                >
                  {t("quizSolver.submitAttemptBtn")}
                </PrimaryButton>
              </div>
            </div>
          ) : (
            /* Main Question Card */
            <div className="w-full bg-card rounded-xl border border-border shadow-sm overflow-hidden">
              <div className="bg-muted/60 px-6 py-3 border-b border-border flex justify-between items-center">
                <span className="text-xs font-medium text-muted-foreground">
                  {t("quizSolver.questionProgress", { current: currentQuestionIndex + 1, total: qCount })}
                </span>
                <div className="flex items-center gap-2">
                  {getTypeBadge(question.type)}
                  {getDifficultyBadge(question.difficulty)}
                </div>
              </div>

              <div className="h-1 bg-muted">
                <div
                  className="h-full bg-primary transition-all duration-300"
                  style={{ width: `${((currentQuestionIndex + 1) / qCount) * 100}%` }}
                />
              </div>

              <div className="p-6 md:p-8 space-y-6">
                <h3 className="text-base font-bold text-foreground leading-relaxed">
                  {question.content}
                </h3>

                {/* Options */}
                <div className="space-y-2.5">
                  {question.options.map((opt) => {
                    const isSelected = currentQuestionSelected.includes(opt.id);

                    let optionStyle = "border-border hover:bg-muted/40 hover:border-primary/30 text-foreground";
                    if (isSelected) {
                      optionStyle = "bg-primary/10 border-primary text-foreground font-medium";
                    }

                    return (
                      <button
                        key={opt.id}
                        onClick={() => handleToggleOption(question.id, opt.id, isMultiple)}
                        className={`w-full p-4 rounded-xl text-left text-[13px] border transition-all flex items-start gap-3.5 cursor-pointer ${optionStyle}`}
                      >
                        <div className="mt-0.5 flex-shrink-0">
                          <div className={`w-4.5 h-4.5 ${isMultiple ? "rounded-md" : "rounded-full"} border flex items-center justify-center transition-colors ${
                            isSelected 
                              ? "bg-primary border-primary text-white" 
                              : "border-border bg-card"
                          }`}>
                            {isSelected && <Check size={11} strokeWidth={3} />}
                          </div>
                        </div>
                        <span className="flex-1 leading-relaxed">{opt.content}</span>
                      </button>
                    );
                  })}
                </div>

                {/* Bottom Navigation */}
                <div className="pt-5 border-t border-border flex items-center justify-between gap-3">
                  <SecondaryButton
                    size="sm"
                    onClick={() => setCurrentQuestionIndex(prev => Math.max(0, prev - 1))}
                    disabled={currentQuestionIndex === 0}
                    icon={<ChevronLeft size={15} />}
                  >
                    {t("quizSolver.prevBtn")}
                  </SecondaryButton>

                  {currentQuestionIndex < qCount - 1 ? (
                    <SecondaryButton
                      size="sm"
                      onClick={() => setCurrentQuestionIndex(prev => prev + 1)}
                      icon={<ChevronRight size={15} />}
                    >
                      {t("quizSolver.nextBtn")}
                    </SecondaryButton>
                  ) : (
                    <PrimaryButton
                      size="sm"
                      onClick={() => setIsReviewMode(true)}
                      icon={<ChevronRight size={15} />}
                      className="px-5 font-semibold"
                    >
                      {t("quizSolver.goToReviewBtn")}
                    </PrimaryButton>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Right Sidebar: Timer + Question Selector Tiles */}
        <div className="shrink-0 sticky top-6 self-start space-y-3">
          {attemptDetails.expiresAt && !finished && (
            <div
              className={`px-3 py-2 rounded-xl border flex items-center justify-center gap-2 shadow-xs transition-all ${
                isExpired
                  ? "bg-rose-500/15 border-rose-500/30 text-rose-500 font-bold"
                  : isTimeLow
                    ? "bg-rose-500/15 border-rose-500/30 text-rose-500 font-bold animate-pulse"
                    : "bg-card border-border text-foreground font-semibold"
              }`}
            >
              <Clock size={15} strokeWidth={2.25} className={isExpired || isTimeLow ? "text-rose-500" : "text-primary"} />
              <span className="text-xs font-bold tabular-nums">
                {isExpired ? "00:00" : timeLeftFormatted}
              </span>
            </div>
          )}

          <div className="grid grid-cols-5 gap-2">
            {attemptDetails.questions.map((q, idx) => {
              const isCurrent = !isReviewMode && idx === currentQuestionIndex;
              const isAnswered = (selectedAnswers[q.id]?.length || 0) > 0;

              let pillStyle = "bg-card border-border text-muted-foreground hover:border-primary/40 hover:text-foreground";
              if (isCurrent) {
                pillStyle = "bg-primary text-white border-primary shadow-xs ring-2 ring-primary/30 font-bold";
              } else if (isAnswered) {
                pillStyle = "bg-primary/15 text-primary border-primary/40 font-semibold hover:bg-primary/25";
              }

              return (
                <button
                  key={q.id}
                  onClick={() => {
                    setCurrentQuestionIndex(idx);
                    setIsReviewMode(false);
                  }}
                  className={`w-9 h-9 rounded-lg border text-xs flex items-center justify-center transition-all cursor-pointer select-none ${pillStyle}`}
                >
                  {idx + 1}
                </button>
              );
            })}
          </div>
        </div>
      </div>

      <ConfirmModal
        isOpen={showSubmitModal}
        onClose={() => setShowSubmitModal(false)}
        onConfirm={handleConfirmSubmit}
        title={t("quizSolver.submitModalTitle")}
        message={t("quizSolver.submitModalMessage")}
        confirmBtnText={t("quizSolver.submitModalConfirm")}
        isDestructive={false}
      />
    </div>
  );
}
