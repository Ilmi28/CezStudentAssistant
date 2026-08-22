import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  ChevronLeft,
  Play,
  RefreshCw,
  Trophy,
  Target,
  HelpCircle,
  Clock,
  ArrowRight,
  CheckCircle2,
  AlertCircle
} from "lucide-react";
import { quizService, QuizAttemptStatus, type QuizDetailsDto } from "../services";
import Card from "../components/Card";
import { PrimaryButton, SecondaryButton } from "../components/Button";

interface QuizDetailsPageProps {
  setError: (msg: string) => void;
}

export default function QuizDetailsPage({ setError }: QuizDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [quiz, setQuiz] = useState<QuizDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [isNavigatingBack, setIsNavigatingBack] = useState(false);

  useEffect(() => {
    loadQuizDetails();
  }, [id]);

  const loadQuizDetails = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const details = await quizService.getQuizDetails(id);
      setQuiz(details);
    } catch (err) {
      console.warn("[QuizDetailsPage] Failed to load quiz details:", err);
      setError(t("quizDetails.loadingDetails"));
      navigate("/quizzes");
    } finally {
      setLoading(false);
    }
  };

  const handleStartNewAttempt = async () => {
    if (!id || starting) return;
    setStarting(true);
    try {
      const attemptDetails = await quizService.startQuiz(id);
      navigate(`/quiz/attempt/${attemptDetails.attemptId}`, {
        state: { initialAttempt: attemptDetails }
      });
    } catch (err) {
      console.warn("[QuizDetailsPage] Failed to start quiz attempt:", err);
      setError(t("quizDetails.starting"));
    } finally {
      setStarting(false);
    }
  };

  const handleContinueAttempt = (attemptId: string) => {
    navigate(`/quiz/attempt/${attemptId}`);
  };

  const handleGoBack = () => {
    setIsNavigatingBack(true);
    setTimeout(() => {
      navigate(-1);
    }, 180);
  };

  if (loading || !quiz) {
    return (
      <div className="flex justify-center py-16">
        <div className="flex items-center gap-3 bg-card px-6 py-4 rounded-xl border border-border shadow-sm">
          <RefreshCw size={18} className="animate-spin text-primary" />
          <span className="text-xs font-medium text-muted-foreground">{t("quizDetails.loadingDetails")}</span>
        </div>
      </div>
    );
  }

  const totalPointsMax = quiz.questions.reduce((sum, q) => sum + Number(q.points), 0);
  const completedAttempts = quiz.attempts.filter(a => a.status === QuizAttemptStatus.Completed);
  const inProgressAttempt = quiz.attempts.find(a => a.status === QuizAttemptStatus.InProgress);

  const bestScore = completedAttempts.length > 0
    ? Math.max(...completedAttempts.map(a => Number(a.points ?? 0)))
    : null;

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
          {inProgressAttempt ? (
            <PrimaryButton
              onClick={() => handleContinueAttempt(inProgressAttempt.id)}
              icon={<Play size={16} strokeWidth={2.25} />}
              className="py-2.5 px-4 text-xs font-semibold"
            >
              {t("quizDetails.continueAttempt")}
            </PrimaryButton>
          ) : (
            <PrimaryButton
              loading={starting}
              onClick={handleStartNewAttempt}
              icon={<Play size={16} strokeWidth={2.25} />}
              className="py-2.5 px-4 text-xs font-semibold"
            >
              {t("quizDetails.startNewAttempt")}
            </PrimaryButton>
          )}
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

          {/* Estimated Time */}
          <div className="flex items-center gap-3.5 pt-4 md:pt-0 md:pl-6">
            <div className="w-10 h-10 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Clock size={20} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[11px] uppercase tracking-wider text-muted-foreground font-semibold block">
                {t("quizDetails.stats.estimatedTime")}
              </span>
              <span className="text-xl font-bold text-foreground">
                ~{Math.max(1, Math.round(quiz.questions.length * 1.5))} min
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
                {totalPointsMax} {t("quizDetails.stats.pts")}
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
                {bestScore !== null ? `${bestScore.toFixed(1)} / ${totalPointsMax}` : "-"}
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
          {inProgressAttempt && (
            <button
              type="button"
              onClick={handleStartNewAttempt}
              disabled={starting}
              className="text-xs text-primary hover:underline font-semibold cursor-pointer"
            >
              + {t("quizDetails.startNewAttempt")}
            </button>
          )}
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
            {quiz.attempts
              .slice()
              .reverse()
              .map((attempt, index) => {
                const attemptNumber = quiz.attempts.length - index;
                const earnedPoints = Number(attempt.points ?? 0);
                const isCompleted = attempt.status === QuizAttemptStatus.Completed;
                const isInProgress = attempt.status === QuizAttemptStatus.InProgress;

                return (
                  <div
                    key={attempt.id}
                    className="p-4 rounded-xl bg-background/50 border border-border flex items-center justify-between gap-4 transition-colors hover:border-primary/30"
                  >
                    <div className="flex items-center gap-3.5 min-w-0">
                      <div className="w-9 h-9 rounded-lg bg-card border border-border flex items-center justify-center text-xs font-bold text-foreground shrink-0">
                        #{attemptNumber}
                      </div>

                      <div className="min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="text-xs font-bold text-foreground">
                            {t("quizDetails.attemptNumber", { number: attemptNumber })}
                          </span>

                          {isCompleted && (
                            <span className="inline-flex items-center gap-1 text-[10px] font-semibold bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 px-2 py-0.5 rounded-full uppercase">
                              <CheckCircle2 size={11} />
                              {t("quizDetails.status.completed")}
                            </span>
                          )}

                          {isInProgress && (
                            <span className="inline-flex items-center gap-1 text-[10px] font-semibold bg-amber-500/15 text-amber-600 dark:text-amber-400 px-2 py-0.5 rounded-full uppercase">
                              <AlertCircle size={11} />
                              {t("quizDetails.status.inProgress")}
                            </span>
                          )}
                        </div>

                        <div className="text-[11px] text-muted-foreground mt-0.5">
                          {t("quizDetails.startedAt")}: {new Date(attempt.startedAt).toLocaleString()}
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 shrink-0">
                      {isCompleted && (
                        <div className="text-right">
                          <div className="text-sm font-bold text-primary">
                            {earnedPoints.toFixed(1)} / {totalPointsMax} pkt
                          </div>
                          <div className="text-[10px] text-muted-foreground">
                            {totalPointsMax > 0 ? `${((earnedPoints / totalPointsMax) * 100).toFixed(0)}%` : "0%"}
                          </div>
                        </div>
                      )}

                      {isInProgress && (
                        <SecondaryButton
                          onClick={() => handleContinueAttempt(attempt.id)}
                          icon={<ArrowRight size={13} />}
                          size="sm"
                          className="py-1.5 px-3 text-xs"
                        >
                          {t("quizDetails.actionContinue")}
                        </SecondaryButton>
                      )}
                    </div>
                  </div>
                );
              })}
          </div>
        )}
      </div>
    </div>
  );
}
