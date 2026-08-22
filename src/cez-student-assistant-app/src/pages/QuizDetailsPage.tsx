import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  ChevronLeft,
  Play,
  Pencil,
  Trophy,
  Target,
  HelpCircle,
  Clock
} from "lucide-react";
import { quizService, QuizAttemptStatus, type QuizDetailsDto } from "../services";
import Card from "../components/Card";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import LoadingScreen from "../components/LoadingScreen";
import EditQuizModal from "../components/EditQuizModal";
import { useCountdown, useQuiz } from "../hooks";

interface QuizDetailsPageProps {
  setError: (msg: string) => void;
  setSuccess?: (msg: string) => void;
}

function AttemptCountdownBadge({
  expiresAt,
  earnedPoints,
  totalPointsMax,
  onExpire,
}: {
  expiresAt: string;
  earnedPoints: number;
  totalPointsMax: number;
  onExpire?: () => void;
}) {
  const { t } = useTranslation();
  const { formatted, isExpired, isTimeLow } = useCountdown(expiresAt, onExpire);

  if (isExpired) {
    const attemptPercentage = totalPointsMax > 0 ? Math.round((earnedPoints / totalPointsMax) * 100) : 0;
    const formatScore = (val: number) => {
      if (Number.isInteger(val)) return val.toString();
      return parseFloat(val.toFixed(2)).toString();
    };

    return (
      <div className="flex flex-col items-end justify-center text-right">
        <span
          className={`text-base md:text-lg font-bold tabular-nums leading-tight ${
            attemptPercentage >= 50
              ? "text-emerald-500 dark:text-emerald-400"
              : "text-amber-500 dark:text-amber-400"
          }`}
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
      <span
        className={`text-base md:text-lg font-bold tabular-nums leading-tight ${
          isTimeLow
            ? "text-rose-500 font-bold animate-pulse"
            : "text-amber-500 dark:text-amber-400"
        }`}
      >
        {formatted}
      </span>
      <span className="text-[11px] text-muted-foreground font-medium">
        {t("quizDetails.status.inProgress", "W toku")}
      </span>
    </div>
  );
}

export default function QuizDetailsPage({ setError, setSuccess }: QuizDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { refreshQuizzes } = useQuiz();

  const [quiz, setQuiz] = useState<QuizDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [isNavigatingBack, setIsNavigatingBack] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  useEffect(() => {
    if (!id) return;
    loadQuizDetails(id);
  }, [id]);

  const loadQuizDetails = async (quizId: string) => {
    setLoading(true);
    try {
      const data = await quizService.getQuizDetails(quizId);
      setQuiz(data);
      refreshQuizzes().catch(() => {});
    } catch (err) {
      console.warn("[QuizDetailsPage] Failed to load quiz details:", err);
      setError(t("common.genericError"));
      navigate("/quizzes");
    } finally {
      setLoading(false);
    }
  };

  const handleGoBack = () => {
    setIsNavigatingBack(true);
    setTimeout(() => {
      navigate("/quizzes");
    }, 200);
  };

  const handleStartNewAttempt = async () => {
    if (!quiz) return;
    setStarting(true);
    try {
      const attempt = await quizService.startQuiz(quiz.id);
      refreshQuizzes().catch(() => {});
      navigate(`/quiz/attempt/${attempt.attemptId}`, {
        state: { initialAttempt: attempt }
      });
    } catch (err) {
      console.warn("[QuizDetailsPage] Failed to start quiz:", err);
      setError(t("common.genericError"));
    } finally {
      setStarting(false);
    }
  };

  const handleContinueAttempt = (attemptId: string) => {
    navigate(`/quiz/attempt/${attemptId}`);
  };

  const handleEditQuizSubmit = async (displayName: string, timeLimitMinutes?: number | null) => {
    if (!id) return;
    await quizService.updateQuiz(id, displayName, timeLimitMinutes);
    refreshQuizzes().catch(() => {});
    if (setSuccess) setSuccess(t("quizDetails.editSuccess"));
    await loadQuizDetails(id);
  };

  if (loading || !quiz) {
    return <LoadingScreen message={t("quizDetails.loadingDetails")} />;
  }

  const totalPointsMax = quiz.maxPoints ?? 0;
  const completedAttempts = quiz.attempts.filter(a => a.status === QuizAttemptStatus.Completed);

  const bestScore = completedAttempts.length > 0
    ? Math.max(...completedAttempts.map(a => Number(a.points ?? 0)))
    : null;

  const formatScore = (val: number) => {
    if (Number.isInteger(val)) return val.toString();
    return parseFloat(val.toFixed(2)).toString();
  };

  return (
    <div
      className={`max-w-4xl mx-auto space-y-6 ${
        isNavigatingBack
          ? "animate-slide-out-right"
          : "animate-in fade-in duration-300"
      }`}
    >
      {/* Header & Actions */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3.5 min-w-0">
          <button
            type="button"
            onClick={handleGoBack}
            title={t("quizDetails.backBtn")}
            aria-label={t("quizDetails.backBtn")}
            className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors shadow-xs cursor-pointer shrink-0"
          >
            <ChevronLeft size={22} strokeWidth={2.25} className="shrink-0" />
          </button>
          <div className="flex flex-col justify-center min-w-0">
            {quiz.courseName && (
              <span className="self-start px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20 mb-1">
                {quiz.courseName}
              </span>
            )}
            <h1 className="text-xl font-bold text-foreground truncate">
              {quiz.displayName || quiz.name}
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
      <Card className="p-5">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 md:gap-6 divide-y md:divide-y-0 md:divide-x divide-border/60">
          {/* Questions */}
          <div className="flex items-center gap-3.5 pt-2 md:pt-0">
            <div className="w-10 h-10 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <HelpCircle size={20} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[11px] uppercase tracking-wider text-muted-foreground font-semibold block">
                {t("quizDetails.stats.questions")}
              </span>
              <span className="text-xl font-bold text-foreground">
                {quiz.questions.length}
              </span>
            </div>
          </div>

          {/* Time Limit */}
          <div className="flex items-center gap-3.5 pt-4 md:pt-0 md:pl-6">
            <div className="w-10 h-10 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Clock size={20} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[11px] uppercase tracking-wider text-muted-foreground font-semibold block">
                {t("quizDetails.stats.timeLimit")}
              </span>
              <span className="text-xl font-bold text-foreground">
                {quiz.timeLimitMinutes ? `${quiz.timeLimitMinutes} min` : t("quizDetails.stats.noLimit")}
              </span>
            </div>
          </div>

          {/* Max Points */}
          <div className="flex items-center gap-3.5 pt-4 md:pt-0 md:pl-6">
            <div className="w-10 h-10 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Target size={20} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[11px] uppercase tracking-wider text-muted-foreground font-semibold block">
                {t("quizDetails.stats.totalPoints")}
              </span>
              <span className="text-xl font-bold text-foreground">
                {formatScore(totalPointsMax)} {t("quizDetails.stats.pts")}
              </span>
            </div>
          </div>

          {/* Best Score */}
          <div className="flex items-center gap-3.5 pt-4 md:pt-0 md:pl-6">
            <div className="w-10 h-10 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Trophy size={20} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[11px] uppercase tracking-wider text-muted-foreground font-semibold block">
                {t("quizDetails.stats.bestScore")}
              </span>
              <span className="text-xl font-bold text-foreground">
                {bestScore !== null && totalPointsMax > 0
                  ? `${Math.round((bestScore / totalPointsMax) * 100)}%`
                  : "-"}
              </span>
            </div>
          </div>
        </div>
      </Card>

      {/* Attempts History Section */}
      <div className="bg-card rounded-xl border border-border p-5 shadow-sm space-y-4">
        <div className="flex items-center justify-between border-b border-border pb-3">
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
            {t("quizDetails.attemptsHistory")} ({quiz.attempts.length})
          </h3>
        </div>

        {quiz.attempts.length === 0 ? (
          <div className="py-10 text-center space-y-3">
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
                const attemptPercentage =
                  totalPointsMax > 0 ? Math.round((earnedPoints / totalPointsMax) * 100) : 0;

                return (
                  <div
                    key={attempt.id}
                    onClick={() => handleContinueAttempt(attempt.id)}
                    className="p-4 rounded-xl bg-background/50 border border-border flex items-center justify-between gap-4 transition-colors hover:border-primary/40 hover:bg-muted/30 cursor-pointer"
                  >
                    <div className="flex items-center gap-3.5 min-w-0">
                      <div className="w-9 h-9 rounded-lg bg-card border border-border flex items-center justify-center text-xs font-bold text-foreground shrink-0">
                        #{attemptNumber}
                      </div>

                      <div className="min-w-0">
                        <span className="text-xs font-bold text-foreground block">
                          {t("quizDetails.attemptNumber", { number: attemptNumber })}
                        </span>

                        <div className="text-[11px] text-muted-foreground mt-0.5">
                          {t("quizDetails.startedAt")}: {new Date(attempt.startedAt).toLocaleString()}
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 shrink-0">
                      {isCompleted && (
                        <div className="flex flex-col items-end justify-center text-right">
                          <span
                            className={`text-base md:text-lg font-bold tabular-nums leading-tight ${
                              attemptPercentage >= 50
                                ? "text-emerald-500 dark:text-emerald-400"
                                : "text-amber-500 dark:text-amber-400"
                            }`}
                          >
                            {attemptPercentage}%
                          </span>
                          <span className="text-[11px] text-muted-foreground font-medium tabular-nums">
                            {formatScore(earnedPoints)} / {formatScore(totalPointsMax)} pkt
                          </span>
                        </div>
                      )}

                      {isInProgress && (
                        attempt.expiresAt ? (
                          <AttemptCountdownBadge
                            expiresAt={attempt.expiresAt}
                            earnedPoints={earnedPoints}
                            totalPointsMax={totalPointsMax}
                            onExpire={() => id && loadQuizDetails(id)}
                          />
                        ) : (
                          <div className="flex flex-col items-end justify-center text-right">
                            <span className="text-base md:text-lg font-bold text-amber-500 dark:text-amber-400 leading-tight">
                              {t("quizDetails.status.inProgress", "W toku")}
                            </span>
                            <span className="text-[11px] text-muted-foreground font-medium">
                              {t("quizDetails.stats.noLimit", "Brak limitu")}
                            </span>
                          </div>
                        )
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
        initialDisplayName={quiz.displayName || quiz.name}
        initialTimeLimitMinutes={quiz.timeLimitMinutes}
      />
    </div>
  );
}
