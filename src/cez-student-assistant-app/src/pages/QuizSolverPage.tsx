import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ChevronLeft, Clock } from "lucide-react";
import { quizService, QuizAttemptStatus, QuestionDifficulty, QuestionType, type QuizAttemptDetailsDto, type QuestionDto } from "../services";
import { PrimaryButton, SecondaryButton, Badge, ConfirmModal, LoadingScreen } from "../components";
import { getScoreColorClass } from "../utils/scoreUtils";

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
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [isReviewMode, setIsReviewMode] = useState(false);
  const [selectedAnswers, setSelectedAnswers] = useState<Record<string, string[]>>({});
  const [finished, setFinished] = useState(false);
  const [finalScore, setFinalScore] = useState(0);
  const [showSubmitModal, setShowSubmitModal] = useState(false);

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
        setCurrentQuestionIndex(
          firstUnansweredIndex !== -1 ? firstUnansweredIndex : Math.max(0, details.questions.length - 1)
        );
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
      <span className="text-xs text-muted-foreground font-medium">
        {isMultiple ? t("quizSolver.type.multiple") : t("quizSolver.type.single")}
      </span>
    );
  };

  const getDifficultyBadge = (difficulty?: QuestionDifficulty) => {
    switch (difficulty) {
      case QuestionDifficulty.Easy:
        return (
          <span className="text-[11px] font-bold text-emerald-600 dark:text-emerald-400 uppercase tracking-wider">
            {t("quizSolver.difficulty.easy")}
          </span>
        );
      case QuestionDifficulty.Hard:
        return (
          <span className="text-[11px] font-bold text-rose-600 dark:text-rose-400 uppercase tracking-wider">
            {t("quizSolver.difficulty.hard")}
          </span>
        );
      case QuestionDifficulty.Medium:
      default:
        return (
          <span className="text-[11px] font-bold text-amber-600 dark:text-amber-400 uppercase tracking-wider">
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
      updatedSelected = currentSelected.includes(optionId) ? [] : [optionId];
    }

    setSelectedAnswers(prev => ({
      ...prev,
      [questionId]: updatedSelected
    }));

    try {
      await quizService.submitAnswer(
        attemptDetails.attemptId,
        questionId,
        updatedSelected
      );
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to save answer:", err);
    }
  };

  const handleConfirmSubmit = async () => {
    if (!attemptDetails) return;

    try {
      await quizService.completeQuizAttempt(attemptDetails.attemptId);
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
          <SecondaryButton
            type="button"
            onClick={handleCancel}
            aria-label={t("quizDetails.backBtn")}
            icon={<ChevronLeft size={22} strokeWidth={2.25} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          />
          <div className="flex flex-col justify-center min-w-0">
            {attemptDetails.courseName && (
              <Badge variant="secondary" className="self-start mb-1">
                {attemptDetails.courseName}
              </Badge>
            )}
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
                    <span className={`text-base font-bold tabular-nums ${getScoreColorClass(scorePercent)}`}>
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

                return (
                  <div
                    key={q.id}
                    id={`report-question-${idx}`}
                    className="p-4 md:p-5 rounded-xl bg-card border border-border space-y-3"
                  >
                    <div className="space-y-1.5">
                      <div className="flex items-center justify-between gap-2">
                        <div className="flex items-center gap-2">
                          {getTypeBadge(q.type)}
                          {getDifficultyBadge(q.difficulty)}
                        </div>
                        <span
                          className={`text-xs font-bold tabular-nums ${
                            isFull
                              ? "text-emerald-400"
                              : isPartial
                                ? "text-amber-400"
                                : "text-rose-400"
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

                        let optionStyle = "border-border/60 text-foreground/80 bg-background/40";

                        if (isSelected && isCorrectOption) {
                          optionStyle = "bg-emerald-500/15 border-emerald-500/40 text-emerald-700 dark:text-emerald-300 font-semibold";
                        } else if (isSelected && !isCorrectOption) {
                          optionStyle = "bg-rose-500/15 border-rose-500/40 text-rose-700 dark:text-rose-300 font-semibold";
                        } else if (!isSelected && isCorrectOption) {
                          optionStyle = "border-emerald-500/35 bg-emerald-500/5 text-emerald-700 dark:text-emerald-400 font-medium";
                        }

                        return (
                          <div
                            key={opt.id}
                            className={`p-3 rounded-lg text-xs md:text-[13px] border transition-colors ${optionStyle}`}
                          >
                            <span className="leading-relaxed block">{opt.content}</span>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Right sticky question tiles - sleek obsidian dark cards with status dots */}
          <div className="grid grid-cols-5 gap-2 shrink-0 sticky top-6 self-start">
            {attemptDetails.questions.map((q, idx) => {
              const userSelected = selectedAnswers[q.id] || [];
              const earnedPoints = calculateQuestionPoints(q, userSelected);
              const maxPoints = getDifficultyPoints(q.difficulty);
              const isFull = earnedPoints === maxPoints;
              const isPartial = earnedPoints > 0 && earnedPoints < maxPoints;

              const ringBorder = isFull
                ? "border-emerald-500/70 dark:border-emerald-500/60"
                : isPartial
                  ? "border-amber-500/70 dark:border-amber-500/60"
                  : "border-rose-500/70 dark:border-rose-500/60";

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
                  className={`w-9 h-9 md:w-10 md:h-10 rounded-xl bg-card border ${ringBorder} text-foreground font-bold text-xs md:text-sm flex items-center justify-center transition-all duration-200 ease-out hover:scale-105 active:scale-95 cursor-pointer select-none shadow-2xs`}
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
      {/* Top Header */}
      <div>
        <SecondaryButton
          type="button"
          onClick={handleCancel}
          aria-label={t("quizDetails.backBtn")}
          icon={<ChevronLeft size={22} strokeWidth={2.25} />}
          className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
        />
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
                              className={`p-3 rounded-lg text-xs md:text-[13px] border transition-colors duration-200 ease-out ${
                                isSelected
                                  ? "bg-primary/10 border-primary/50 text-foreground font-semibold"
                                  : "border-border/60 text-foreground/80 bg-card/40"
                              }`}
                            >
                              <span className="leading-relaxed block">{opt.content}</span>
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
            <div className="w-full space-y-4">
              {/* Progress Bar - matching flashcards 1:1 */}
              <div className="w-full h-2 bg-secondary rounded-full overflow-hidden border border-border/60">
                <div
                  className="h-full bg-primary transition-all duration-300 ease-out"
                  style={{ width: `${((currentQuestionIndex + 1) / qCount) * 100}%` }}
                />
              </div>

              <div className="w-full bg-card rounded-xl border border-border shadow-sm overflow-hidden">
                <div className="bg-muted/60 px-6 py-3 border-b border-border flex justify-between items-center">
                  <div className="flex items-center gap-2">
                    {getTypeBadge(question.type)}
                  </div>
                  <div>
                    {getDifficultyBadge(question.difficulty)}
                  </div>
                </div>

                <div key={currentQuestionIndex} className="p-6 md:p-8 space-y-6 animate-in fade-in-50 slide-in-from-right-3 duration-300 ease-out">
                  <h3 className="text-base font-bold text-foreground leading-relaxed">
                    {question.content}
                  </h3>

                  {/* Options */}
                  <div className="space-y-2.5">
                    {question.options.map((opt) => {
                      const isSelected = currentQuestionSelected.includes(opt.id);

                      let optionStyle = "border-border/70 bg-card/40 hover:bg-card hover:border-border text-foreground/80";
                      if (isSelected) {
                        optionStyle = "bg-primary/10 border-primary/60 text-foreground font-semibold shadow-xs";
                      }

                      return (
                        <button
                          key={opt.id}
                          type="button"
                          onClick={() => handleToggleOption(question.id, opt.id, isMultiple)}
                          className={`w-full p-3.5 md:p-4 rounded-xl text-left text-xs md:text-[13px] border transition-colors duration-200 ease-out cursor-pointer ${optionStyle}`}
                        >
                          <span className="leading-relaxed block">{opt.content}</span>
                        </button>
                      );
                    })}
                  </div>

                  {/* Bottom Navigation */}
                  <div className="pt-2 flex items-center justify-between gap-3">
                    <SecondaryButton
                      size="sm"
                      onClick={() => setCurrentQuestionIndex(prev => Math.max(0, prev - 1))}
                      disabled={currentQuestionIndex === 0}
                    >
                      {t("quizSolver.prevBtn")}
                    </SecondaryButton>

                    {currentQuestionIndex < qCount - 1 ? (
                      <PrimaryButton
                        size="sm"
                        onClick={() => setCurrentQuestionIndex(prev => prev + 1)}
                        className="px-5 font-semibold"
                      >
                        {t("quizSolver.nextBtn")}
                      </PrimaryButton>
                    ) : (
                      <PrimaryButton
                        size="sm"
                        onClick={() => setIsReviewMode(true)}
                        className="px-5 font-semibold"
                      >
                        {t("quizSolver.goToReviewBtn")}
                      </PrimaryButton>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Right Sidebar: Timer + Question Selector Tiles */}
        <div className="shrink-0 sticky top-6 self-start pt-6 space-y-3">
          {attemptDetails.timeLimitMinutes != null && attemptDetails.timeLimitMinutes > 0 && !finished && (
            <div className="px-3 py-2 rounded-xl border flex items-center justify-center gap-2 shadow-xs bg-card border-border text-foreground font-semibold">
              <Clock size={15} strokeWidth={2.25} className="text-primary" />
              <span className="text-xs font-bold tabular-nums">
                {attemptDetails.timeLimitMinutes} min
              </span>
            </div>
          )}

          <div className="grid grid-cols-5 gap-2">
            {attemptDetails.questions.map((q, idx) => {
              const isCurrent = !isReviewMode && idx === currentQuestionIndex;
              const isAnswered = (selectedAnswers[q.id]?.length || 0) > 0;

              let ringBorder = "border-border text-muted-foreground/60 font-medium hover:border-border/80 hover:text-foreground";
              if (isCurrent) {
                ringBorder = "border-2 border-primary text-primary font-bold shadow-xs";
              } else if (isAnswered) {
                ringBorder = "border border-primary/60 text-foreground font-bold hover:border-primary/80";
              }

              return (
                <button
                  key={q.id}
                  onClick={() => {
                    setCurrentQuestionIndex(idx);
                    setIsReviewMode(false);
                  }}
                  className={`w-9 h-9 md:w-10 md:h-10 rounded-xl bg-card ${ringBorder} text-xs md:text-sm flex items-center justify-center transition-all duration-200 ease-out hover:scale-105 active:scale-95 cursor-pointer select-none shadow-2xs`}
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
