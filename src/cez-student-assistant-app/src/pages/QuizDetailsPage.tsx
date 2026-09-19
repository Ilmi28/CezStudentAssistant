import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { formatDateTime } from "../helpers/dateHelper";
import {
  ChevronLeft,
  Play,
  Pencil,
  Target,
  HelpCircle,
  Clock,
  Layers
} from "lucide-react";
import { quizService, QuizAttemptStatus, QuestionDifficulty, QuestionType, type QuizDetailsDto } from "../services";
import {
  Card,
  Badge,
  PrimaryButton,
  SecondaryButton,
  LoadingScreen,
  EditQuizModal,
  MultiSegmentProgressBar,
  Tooltip,
} from "../components";
import { getScoreColorClass } from "../utils/scoreUtils";

interface QuizDetailsPageProps {
  setError: (msg: string) => void;
}

function formatDuration(startedAt: string, completedAt?: string | null): string | null {
  if (!completedAt) return null;
  const start = new Date(startedAt).getTime();
  const end = new Date(completedAt).getTime();
  const diffSec = Math.max(0, Math.floor((end - start) / 1000));
  if (diffSec === 0) return null;

  const mins = Math.floor(diffSec / 60);
  const secs = diffSec % 60;

  if (mins === 0) {
    return `${secs} s`;
  }
  if (secs === 0) {
    return `${mins} min`;
  }
  return `${mins} min ${secs} s`;
}

function AttemptBadge({
  isCompleted,
  earnedPoints,
  totalPointsMax,
}: {
  isCompleted: boolean;
  earnedPoints: number;
  totalPointsMax: number;
}) {
  const { t } = useTranslation();

  if (isCompleted) {
    const attemptPercentage = totalPointsMax > 0 ? Math.round((earnedPoints / totalPointsMax) * 100) : 0;
    const formatScore = (val: number) => {
      if (Number.isInteger(val)) return val.toString();
      return parseFloat(val.toFixed(2)).toString();
    };

    return (
      <div className="flex flex-col items-end justify-center text-right">
        <span
          className={`text-base md:text-lg font-bold tabular-nums leading-tight ${getScoreColorClass(attemptPercentage)}`}
        >
          {attemptPercentage}%
        </span>
        <span className="text-[11px] text-muted-foreground font-medium tabular-nums">
          {formatScore(earnedPoints)} / {formatScore(totalPointsMax)} pkt
        </span>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-end justify-center text-right">
      <span className="text-xs font-bold text-amber-500 dark:text-amber-400 uppercase tracking-wider">
        {t("quizDetails.status.inProgress", "W TOKU")}
      </span>
    </div>
  );
}

export default function QuizDetailsPage({ setError }: QuizDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const [quiz, setQuiz] = useState<QuizDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  useEffect(() => {
    if (!id) return;
    loadQuizDetails(id);
  }, [id]);

  const loadQuizDetails = async (quizId: string, showFullLoading = true) => {
    if (showFullLoading) {
      setLoading(true);
    }
    try {
      const data = await quizService.getQuizDetails(quizId);
      setQuiz(data);
    } catch (err) {
      console.warn("[QuizDetailsPage] Failed to load quiz details:", err);
      setError(t("common.genericError"));
      if (location.state?.fromPath) {
        navigate(location.state.fromPath);
      } else {
        navigate("/quizzes");
      }
    } finally {
      if (showFullLoading) {
        setLoading(false);
      }
    }
  };

  const handleGoBack = () => {
    if (location.state?.fromPath) {
      navigate(location.state.fromPath);
    } else if (quiz?.courseId) {
      navigate(`/course/${quiz.courseId}`);
    } else {
      navigate("/quizzes");
    }
  };

  const handleStartNewAttempt = async () => {
    if (!quiz) return;
    setStarting(true);
    try {
      const attempt = await quizService.startQuiz(quiz.id);
      navigate(`/quiz/attempt/${attempt.attemptId}`, {
        state: {
          initialAttempt: attempt,
          fromPath: location.state?.fromPath || (quiz.courseId ? `/course/${quiz.courseId}` : "/quizzes")
        }
      });
    } catch (err) {
      console.warn("[QuizDetailsPage] Failed to start quiz:", err);
      setError(t("common.genericError"));
    } finally {
      setStarting(false);
    }
  };

  const handleContinueAttempt = (attemptId: string) => {
    navigate(`/quiz/attempt/${attemptId}`, {
      state: {
        fromPath: location.state?.fromPath || (quiz?.courseId ? `/course/${quiz.courseId}` : "/quizzes")
      }
    });
  };

  const handleEditQuizSubmit = async (
    name: string,
    timeLimitMinutes?: number | null,
    questionCountPerAttempt?: number | null,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null
  ) => {
    if (!id) return;
    await quizService.updateQuiz(
      id,
      name,
      timeLimitMinutes,
      questionCountPerAttempt,
      easyCount,
      mediumCount,
      hardCount
    );
    await loadQuizDetails(id, false);
  };

  if (loading || !quiz) {
    return <LoadingScreen message={t("quizDetails.loadingDetails")} />;
  }

  const totalPointsMax = quiz.maxPoints ?? 0;
  const completedAttempts = quiz.attempts.filter(a => a.status === QuizAttemptStatus.Completed);

  const masteredQuestionIds = new Set<string>();
  if (quiz.questions.length > 0 && completedAttempts.length > 0) {
    quiz.questions.forEach((q) => {
      const correctOptionIds = q.options.filter((o) => o.isCorrect).map((o) => o.id);
      if (correctOptionIds.length === 0) return;

      const isAnsweredCorrectly = completedAttempts.some((a) => {
        const ans = a.answers.find((ansItem) => ansItem.questionId === q.id);
        if (!ans) return false;
        const selected = ans.selectedOptionIds || [];

        if (q.type === QuestionType.SingleChoice) {
          return selected.length === 1 && correctOptionIds.includes(selected[0]);
        }
        return (
          selected.length === correctOptionIds.length &&
          selected.every((id) => correctOptionIds.includes(id))
        );
      });

      if (isAnsweredCorrectly) {
        masteredQuestionIds.add(q.id);
      }
    });
  }

  const masteredCount = masteredQuestionIds.size;
  const poolTotalCount = quiz.questions.length;
  const masteryPercentage = poolTotalCount > 0 ? Math.round((masteredCount / poolTotalCount) * 100) : 0;

  const formatScore = (val: number) => {
    if (Number.isInteger(val)) return val.toString();
    return parseFloat(val.toFixed(2)).toString();
  };

  const isEasy = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Easy || diff === 1 || diff === "Easy" || diff === "1";

  const isHard = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Hard || diff === 3 || diff === "Hard" || diff === "3";

  const easyInPool = quiz.questions.filter(q => isEasy(q.difficulty)).length;
  const hardInPool = quiz.questions.filter(q => isHard(q.difficulty)).length;
  const mediumInPool = quiz.questions.filter(q => !isEasy(q.difficulty) && !isHard(q.difficulty)).length;

  const easyAttemptCount = quiz.easyQuestionCountPerAttempt ?? easyInPool;
  const mediumAttemptCount = quiz.mediumQuestionCountPerAttempt ?? mediumInPool;
  const hardAttemptCount = quiz.hardQuestionCountPerAttempt ?? hardInPool;

  const totalAttemptConfigured = quiz.questionCountPerAttempt ?? (easyAttemptCount + mediumAttemptCount + hardAttemptCount);
  const configuredAttemptMaxPoints = (quiz.easyQuestionCountPerAttempt != null || quiz.mediumQuestionCountPerAttempt != null || quiz.hardQuestionCountPerAttempt != null)
    ? (easyAttemptCount * 1 + mediumAttemptCount * 2 + hardAttemptCount * 3)
    : totalPointsMax;
  return (
    <div className="w-full space-y-6 animate-in fade-in duration-300">
      {/* Header & Actions */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3.5 min-w-0">
          <SecondaryButton
            type="button"
            onClick={handleGoBack}
            aria-label={t("quizDetails.backBtn")}
            icon={<ChevronLeft size={22} strokeWidth={2.25} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          />
          <div className="flex flex-col justify-center min-w-0">
            {quiz.courseName && (
              <Badge variant="secondary" className="self-start mb-1">
                {quiz.courseName}
              </Badge>
            )}
            <h1 className="text-xl font-bold text-foreground break-words leading-tight">
              {quiz.name}
            </h1>
          </div>
        </div>

        <div className="flex items-center gap-2.5 shrink-0">
          <SecondaryButton
            type="button"
            onClick={() => setIsEditModalOpen(true)}
            icon={<Pencil size={15} strokeWidth={2.25} />}
            className="py-2.5 px-3.5 text-xs font-semibold"
          >
            {t("quizDetails.editBtn")}
          </SecondaryButton>
          <PrimaryButton
            loading={starting}
            onClick={handleStartNewAttempt}
            icon={<Play size={16} strokeWidth={2.25} />}
            className="py-2.5 px-4 text-xs font-semibold"
          >
            {t("quizDetails.startNewAttempt")}
          </PrimaryButton>
        </div>
      </div>

      {/* Unified Stats Card */}
      <Card className="p-4 sm:p-5">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3.5 xl:gap-4 divide-y sm:divide-y-0 sm:divide-x divide-border/50">
          {/* Questions Per Attempt */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-foreground dark:text-white shrink-0">
              <HelpCircle size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.questions")}
              </span>
              <span key={String(quiz.questionCountPerAttempt ?? totalAttemptConfigured)} className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {quiz.questionCountPerAttempt ?? totalAttemptConfigured}
              </span>
            </div>
          </div>

          {/* Question Bank Pool */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-foreground dark:text-white shrink-0">
              <Layers size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.questionPool")}
              </span>
              <span key={String(quiz.questions.length)} className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {quiz.questions.length}
              </span>
            </div>
          </div>

          {/* Time Limit */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-foreground dark:text-white shrink-0">
              <Clock size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.timeLimit")}
              </span>
              <span key={String(quiz.timeLimitMinutes)} className="text-lg font-bold text-foreground block whitespace-nowrap truncate animate-in fade-in zoom-in-95 duration-300">
                {quiz.timeLimitMinutes ? `${quiz.timeLimitMinutes} min` : t("quizDetails.stats.noLimit")}
              </span>
            </div>
          </div>

          {/* Max Points */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-foreground dark:text-white shrink-0">
              <Target size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.totalPoints")}
              </span>
              <span key={String(configuredAttemptMaxPoints)} className="text-lg font-bold text-foreground block whitespace-nowrap truncate animate-in fade-in zoom-in-95 duration-300">
                {formatScore(configuredAttemptMaxPoints)} {t("quizDetails.stats.pts")}
              </span>
            </div>
          </div>
        </div>

        {/* Pool Mastery Progress Bar */}
        {quiz.questions.length > 0 && (
          <div className="mt-4 pt-3.5 border-t border-border/50 space-y-2">
            <div className="flex items-center justify-between text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
              <div className="flex items-center gap-1.5">
                <span>{t("quizDetails.stats.masteredQuestions")}</span>
                <Tooltip content={t("quizDetails.difficultyWeightTooltip")}>
                  <div className="inline-flex items-center cursor-help text-muted-foreground/70 hover:text-foreground transition-colors">
                    <HelpCircle size={13} strokeWidth={2} />
                  </div>
                </Tooltip>
              </div>
              <span className="text-foreground font-bold">
                {masteredCount} / {poolTotalCount} {t("quizDetails.stats.questionsSuffix")} ({masteryPercentage}%)
              </span>
            </div>
            <MultiSegmentProgressBar
              segments={[
                {
                  id: "progres",
                  value: masteredCount,
                  colorClass: "bg-emerald-500",
                  customTooltip: `${t("quizDetails.stats.masteredQuestions")}: ${masteredCount} / ${poolTotalCount} (${masteryPercentage}%)`,
                },
              ]}
              totalValue={poolTotalCount}
              heightClass="h-3.5"
            />
          </div>
        )}

        {/* Bottom Difficulty Distribution Bar */}
        {quiz.questions.length > 0 && (
          <div className="mt-4 pt-3.5 border-t border-border/50 space-y-3">
            <div className="flex items-center justify-between text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
              <span>{t("quizDetails.stats.difficultyBreakdown")}</span>
            </div>

            <MultiSegmentProgressBar
              segments={[
                { id: "easy", value: easyAttemptCount, colorClass: "bg-emerald-500", customTooltip: `${t("quizSolver.difficulty.easy", "Łatwe")} (${easyAttemptCount})` },
                { id: "medium", value: mediumAttemptCount, colorClass: "bg-amber-500", customTooltip: `${t("quizSolver.difficulty.medium", "Średnie")} (${mediumAttemptCount})` },
                { id: "hard", value: hardAttemptCount, colorClass: "bg-rose-500", customTooltip: `${t("quizSolver.difficulty.hard", "Trudne")} (${hardAttemptCount})` },
              ]}
              heightClass="h-3.5"
            />
          </div>
        )}
      </Card>

      {/* Attempts History Section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="text-base font-bold text-foreground">
            {t("quizDetails.attemptsHistory")} ({quiz.attempts.length})
          </h3>
        </div>

        {quiz.attempts.length === 0 ? (
          <div className="bg-card rounded-xl border border-border p-10 text-center space-y-3">
            <Clock size={32} className="mx-auto text-muted-foreground/35 mb-2" />
            <p className="text-sm font-medium text-muted-foreground">{t("quizDetails.noAttempts")}</p>
            <p className="text-xs text-muted-foreground/60">{t("quizDetails.noAttemptsSubtitle")}</p>
            <div className="pt-2">
              <PrimaryButton
                loading={starting}
                onClick={handleStartNewAttempt}
                icon={<Play size={15} strokeWidth={2.25} />}
                className="text-xs py-2 px-5"
              >
                {t("quizDetails.startNewAttempt")}
              </PrimaryButton>
            </div>
          </div>
        ) : (
          <div className="space-y-3">
            {[...quiz.attempts]
              .sort((a, b) => new Date(a.startedAt).getTime() - new Date(b.startedAt).getTime())
              .map((attempt, index) => ({
                ...attempt,
                attemptNumber: index + 1
              }))
              .reverse()
              .map((attempt) => {
                const attemptNumber = attempt.attemptNumber;
                const earnedPoints = Number(attempt.points ?? 0);
                const isCompleted = attempt.status === QuizAttemptStatus.Completed;
                const isInProgress = attempt.status === QuizAttemptStatus.InProgress;
                const attemptMax = attempt.maxPoints ?? totalPointsMax;
                const attemptPercentage =
                  attemptMax > 0 ? Math.round((earnedPoints / attemptMax) * 100) : 0;
                const attemptQuestionCount = attempt.questionCount ?? quiz.questions.length;
                const attemptTimeLimit = attempt.timeLimitMinutes;

                return (
                  <div
                    key={attempt.id}
                    onClick={() => handleContinueAttempt(attempt.id)}
                    className="group card-app-spring p-4 rounded-xl bg-card border border-border flex items-center justify-between gap-4 cursor-pointer select-none"
                  >
                    <div className="flex items-center gap-3.5 min-w-0">
                      <div className="w-9 h-9 rounded-lg bg-background border border-border flex items-center justify-center text-xs font-bold text-foreground shrink-0">
                        #{attemptNumber}
                      </div>

                      <div className="min-w-0">
                        <span className="text-xs font-bold text-foreground block tile-title-scale">
                          {formatDateTime(attempt.startedAt, {
                            day: "numeric",
                            month: "short",
                            year: "numeric",
                            hour: "2-digit",
                            minute: "2-digit",
                          })}
                        </span>

                        <div className="text-[11px] text-muted-foreground mt-0.5 flex items-center gap-2 flex-wrap">
                          <span>{attemptQuestionCount === 1 ? "1 pytanie" : (attemptQuestionCount % 10 >= 2 && attemptQuestionCount % 10 <= 4 && (attemptQuestionCount % 100 < 10 || attemptQuestionCount % 100 >= 20)) ? `${attemptQuestionCount} pytania` : `${attemptQuestionCount} pytań`}</span>
                          <span>•</span>
                          <span>{attemptTimeLimit ? `${attemptTimeLimit} min` : t("quizDetails.stats.noLimit", "Brak limitu")}</span>
                          {formatDuration(attempt.startedAt, attempt.completedAt) && (
                            <>
                              <span>•</span>
                              <span>{formatDuration(attempt.startedAt, attempt.completedAt)}</span>
                            </>
                          )}
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 shrink-0">
                      {isCompleted && (
                        <div className="flex flex-col items-end justify-center text-right">
                          <span
                            className={`text-base md:text-lg font-bold tabular-nums leading-tight ${getScoreColorClass(attemptPercentage)}`}
                          >
                            {attemptPercentage}%
                          </span>
                          <span className="text-[11px] text-muted-foreground font-medium tabular-nums">
                            {formatScore(earnedPoints)} / {formatScore(attemptMax)} pkt
                          </span>
                        </div>
                      )}

                      {isInProgress && (
                        <AttemptBadge
                          isCompleted={false}
                          earnedPoints={earnedPoints}
                          totalPointsMax={attemptMax}
                        />
                      )}
                    </div>
                  </div>
                );
              })}
          </div>
        )}
      </div>

      <EditQuizModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        onSubmit={handleEditQuizSubmit}
        initialName={quiz.name}
        initialTimeLimitMinutes={quiz.timeLimitMinutes}
        initialQuestionCountPerAttempt={quiz.questionCountPerAttempt}
        initialEasyCount={quiz.easyQuestionCountPerAttempt}
        initialMediumCount={quiz.mediumQuestionCountPerAttempt}
        initialHardCount={quiz.hardQuestionCountPerAttempt}
        easyInPool={easyInPool}
        mediumInPool={mediumInPool}
        hardInPool={hardInPool}
      />
    </div>
  );
}
