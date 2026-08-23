import { useState, useEffect, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  ChevronLeft,
  Play,
  Pencil,
  Trophy,
  Target,
  HelpCircle,
  Clock,
  Layers
} from "lucide-react";
import { quizService, QuizAttemptStatus, QuestionDifficulty, type QuizDetailsDto } from "../services";
import Card from "../components/Card";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import LoadingScreen from "../components/LoadingScreen";
import EditQuizModal from "../components/EditQuizModal";
import { useCountdown, useQuiz } from "../hooks";

interface QuizDetailsPageProps {
  setError: (msg: string) => void;
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

export default function QuizDetailsPage({ setError }: QuizDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { refreshQuizzes } = useQuiz();

  const [quiz, setQuiz] = useState<QuizDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [starting, setStarting] = useState(false);
  const [isNavigatingBack, setIsNavigatingBack] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [mousePos, setMousePos] = useState<{ x: number; diff: 'easy' | 'medium' | 'hard' } | null>(null);
  const barContainerRef = useRef<HTMLDivElement>(null);

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

  const handleEditQuizSubmit = async (
    displayName: string,
    timeLimitMinutes?: number | null,
    questionCountPerAttempt?: number | null,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null
  ) => {
    if (!id) return;
    await quizService.updateQuiz(
      id,
      displayName,
      timeLimitMinutes,
      questionCountPerAttempt,
      easyCount,
      mediumCount,
      hardCount
    );
    refreshQuizzes().catch(() => {});
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

  const handleBarMouseMove = (e: React.MouseEvent<HTMLDivElement>, diff: 'easy' | 'medium' | 'hard') => {
    if (!barContainerRef.current) return;
    const rect = barContainerRef.current.getBoundingClientRect();
    const x = e.clientX - rect.left;
    setMousePos({ x, diff });
  };

  const getTooltipText = (diff: 'easy' | 'medium' | 'hard') => {
    if (diff === 'easy') {
      return `${t("quizSolver.difficulty.easy")} - ${easyAttemptCount}`;
    }
    if (diff === 'medium') {
      return `${t("quizSolver.difficulty.medium")} - ${mediumAttemptCount}`;
    }
    return `${t("quizSolver.difficulty.hard")} - ${hardAttemptCount}`;
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
      <Card className="p-4 sm:p-5">
        <div className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-5 gap-3.5 xl:gap-4 divide-y sm:divide-y-0 sm:divide-x divide-border/50">
          {/* Questions Per Attempt */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0">
            <div className="w-9 h-9 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <HelpCircle size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.questions")}
              </span>
              <span className="text-lg font-bold text-foreground block truncate">
                {quiz.questionCountPerAttempt ?? totalAttemptConfigured}
              </span>
            </div>
          </div>

          {/* Question Bank Pool */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Layers size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.questionPool")}
              </span>
              <span className="text-lg font-bold text-foreground block truncate">
                {quiz.questions.length}
              </span>
            </div>
          </div>

          {/* Time Limit */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Clock size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.timeLimit")}
              </span>
              <span className="text-lg font-bold text-foreground block whitespace-nowrap truncate">
                {quiz.timeLimitMinutes ? `${quiz.timeLimitMinutes} min` : t("quizDetails.stats.noLimit")}
              </span>
            </div>
          </div>

          {/* Max Points */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-3 xl:pt-0 sm:pl-0 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Target size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.totalPoints")}
              </span>
              <span className="text-lg font-bold text-foreground block whitespace-nowrap truncate">
                {formatScore(configuredAttemptMaxPoints)} {t("quizDetails.stats.pts")}
              </span>
            </div>
          </div>

          {/* Best Score */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-3 xl:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Trophy size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("quizDetails.stats.bestScore")}
              </span>
              <span className="text-lg font-bold text-foreground block truncate">
                {bestScore !== null && totalPointsMax > 0
                  ? `${Math.round((bestScore / totalPointsMax) * 100)}%`
                  : "-"}
              </span>
            </div>
          </div>
        </div>

        {/* Bottom Difficulty Distribution Bar */}
        {quiz.questions.length > 0 && (
          <div className="mt-4 pt-3.5 border-t border-border/50 space-y-3">
            <div className="flex items-center justify-between text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
              <span>{t("quizDetails.stats.difficultyBreakdown")}</span>
            </div>

            {/* Segmented Interactive Progress Bar */}
            <div
              ref={barContainerRef}
              onMouseLeave={() => setMousePos(null)}
              className="h-3.5 w-full bg-secondary rounded-full flex gap-1 p-0.5 border border-border/50 relative"
            >
              {mousePos && (
                <div
                  style={{ left: `${mousePos.x}px` }}
                  className="absolute bottom-full mb-2.5 -translate-x-1/2 flex flex-col items-center z-30 pointer-events-none transition-none"
                >
                  <div className="px-3 py-1.5 rounded-lg bg-card text-foreground text-xs font-semibold shadow-xl border border-border whitespace-nowrap">
                    {getTooltipText(mousePos.diff)}
                  </div>
                  <div className="w-2.5 h-2.5 -mt-1.5 rotate-45 bg-card border-r border-b border-border" />
                </div>
              )}

              {easyAttemptCount > 0 && (
                <div
                  style={{ width: `${(easyAttemptCount / Math.max(1, totalAttemptConfigured)) * 100}%` }}
                  onMouseMove={(e) => handleBarMouseMove(e, 'easy')}
                  className="h-full bg-emerald-500 rounded-full transition-all duration-200 cursor-pointer hover:brightness-125 hover:scale-y-125"
                />
              )}

              {mediumAttemptCount > 0 && (
                <div
                  style={{ width: `${(mediumAttemptCount / Math.max(1, totalAttemptConfigured)) * 100}%` }}
                  onMouseMove={(e) => handleBarMouseMove(e, 'medium')}
                  className="h-full bg-amber-500 rounded-full transition-all duration-200 cursor-pointer hover:brightness-125 hover:scale-y-125"
                />
              )}

              {hardAttemptCount > 0 && (
                <div
                  style={{ width: `${(hardAttemptCount / Math.max(1, totalAttemptConfigured)) * 100}%` }}
                  onMouseMove={(e) => handleBarMouseMove(e, 'hard')}
                  className="h-full bg-rose-500 rounded-full transition-all duration-200 cursor-pointer hover:brightness-125 hover:scale-y-125"
                />
              )}
            </div>
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
                    className="p-4 rounded-xl bg-card border border-border flex items-center justify-between gap-4 transition-colors hover:border-primary/40 hover:bg-muted/30 cursor-pointer"
                  >
                    <div className="flex items-center gap-3.5 min-w-0">
                      <div className="w-9 h-9 rounded-lg bg-background border border-border flex items-center justify-center text-xs font-bold text-foreground shrink-0">
                        #{attemptNumber}
                      </div>

                      <div className="min-w-0">
                        <span className="text-xs font-bold text-foreground block">
                          {t("quizDetails.attemptNumber", { number: attemptNumber })}
                        </span>

                        <div className="text-[11px] text-muted-foreground mt-0.5 flex items-center gap-2 flex-wrap">
                          <span>{t("quizDetails.startedAt")}: {new Date(attempt.startedAt).toLocaleString()}</span>
                          <span>•</span>
                          <span>{attemptQuestionCount === 1 ? "1 pytanie" : (attemptQuestionCount % 10 >= 2 && attemptQuestionCount % 10 <= 4 && (attemptQuestionCount % 100 < 10 || attemptQuestionCount % 100 >= 20)) ? `${attemptQuestionCount} pytania` : `${attemptQuestionCount} pytań`}</span>
                          <span>•</span>
                          <span>{attemptTimeLimit ? `${attemptTimeLimit} min` : t("quizDetails.stats.noLimit", "Brak limitu")}</span>
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
                            {formatScore(earnedPoints)} / {formatScore(attemptMax)} pkt
                          </span>
                        </div>
                      )}

                      {isInProgress && (
                        attempt.expiresAt ? (
                          <AttemptCountdownBadge
                            expiresAt={attempt.expiresAt}
                            earnedPoints={earnedPoints}
                            totalPointsMax={attemptMax}
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
