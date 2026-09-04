import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { flashcardService } from "../services/flashcardService";
import type { FlashcardDeckDetailsDto } from "../types/flashcardTypes";
import { FlashcardStateEnum } from "../enums/flashcardEnums";
import { QuestionDifficulty, QuizAttemptStatus } from "../enums/quizEnums";
import {
  Card,
  Badge,
  PrimaryButton,
  SecondaryButton,
  Alert,
  LoadingScreen,
  EditFlashcardDeckModal,
  MultiSegmentProgressBar,
  Tooltip,
} from "../components";
import { getScoreColorClass } from "../utils/scoreUtils";
import {
  ChevronLeft,
  Play,
  Pencil,
  Trash2,
  CheckCircle2,
  BookOpen,
  HelpCircle,
  Layers,
  ArrowLeft,
  Clock
} from "lucide-react";

export default function FlashcardDeckDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { t, i18n } = useTranslation();

  const [deck, setDeck] = useState<FlashcardDeckDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const fromPath = (location.state as { fromPath?: string })?.fromPath || (deck?.courseId ? `/course/${deck.courseId}` : "/courses");

  const fetchDeckDetails = async () => {
    if (!id) return;
    setLoading(true);
    setErrorMsg(null);
    try {
      const data = await flashcardService.getFlashcardDeckDetails(id);
      setDeck(data);
    } catch (err: any) {
      setErrorMsg(err.message || t("flashcardDetails.fetchError"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDeckDetails();
  }, [id]);

  const handleGoBack = () => {
    if (location.state?.fromPath) {
      navigate(location.state.fromPath);
    } else if (deck?.courseId) {
      navigate(`/course/${deck.courseId}`);
    } else {
      navigate("/courses");
    }
  };

  const handleDeleteDeck = async () => {
    if (!id || !deck) return;
    if (window.confirm(t("flashcardDetails.deleteConfirm"))) {
      try {
        await flashcardService.deleteFlashcardDeck(id);
        navigate(fromPath);
      } catch (err: any) {
        setErrorMsg(err.message || t("flashcardDetails.deleteError"));
      }
    }
  };

  const handleStartStudyDirect = async () => {
    if (!deck) return;
    const hasSpecificDifficultyConfig =
      deck.easyCardCountPerAttempt != null ||
      deck.mediumCardCountPerAttempt != null ||
      deck.hardCardCountPerAttempt != null;

    const easyCount = hasSpecificDifficultyConfig ? (deck.easyCardCountPerAttempt ?? undefined) : undefined;
    const mediumCount = hasSpecificDifficultyConfig ? (deck.mediumCardCountPerAttempt ?? undefined) : undefined;
    const hardCount = hasSpecificDifficultyConfig ? (deck.hardCardCountPerAttempt ?? undefined) : undefined;

    const totalSelected =
      deck.cardCountPerAttempt ||
      (hasSpecificDifficultyConfig
        ? (deck.easyCardCountPerAttempt ?? 0) + (deck.mediumCardCountPerAttempt ?? 0) + (deck.hardCardCountPerAttempt ?? 0)
        : deck.cards.length);

    try {
      const attempt = await flashcardService.startFlashcardAttempt(deck.id, totalSelected);
      navigate(`/flashcards/${deck.id}/study`, {
        state: {
          fromPath: location.pathname,
          attemptId: attempt.id,
          easyCount,
          mediumCount,
          hardCount
        }
      });
    } catch (err) {
      console.warn("[FlashcardDeckDetailsPage] Failed to start attempt:", err);
      navigate(`/flashcards/${deck.id}/study`, {
        state: {
          fromPath: location.pathname,
          easyCount,
          mediumCount,
          hardCount
        }
      });
    }
  };

  const handleOpenAttempt = (attemptId: string) => {
    if (!id || !deck) return;
    navigate(`/flashcards/${deck.id}/study`, {
      state: {
        fromPath: location.pathname,
        attemptId,
      },
    });
  };

  if (loading) {
    return <LoadingScreen message={t("flashcards.loadingDeckDetails")} />;
  }

  if (errorMsg || !deck) {
    return (
      <div className="flex-1 p-6 md:p-8 max-w-4xl mx-auto space-y-4">
        <SecondaryButton
          onClick={handleGoBack}
          icon={<ArrowLeft size={14} />}
          size="sm"
        >
          {t("flashcardDetails.backBtn")}
        </SecondaryButton>
        <Alert variant="error" message={errorMsg || t("flashcardDetails.notFound")} />
      </div>
    );
  }

  const isEasy = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Easy || diff === 1 || diff === "Easy" || diff === "1";
  const isHard = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Hard || diff === 3 || diff === "Hard" || diff === "3";

  const easyInDeck = deck.cards.filter((c) => isEasy(c.difficulty)).length;
  const hardInDeck = deck.cards.filter((c) => isHard(c.difficulty)).length;
  const mediumInDeck = deck.cards.filter((c) => !isEasy(c.difficulty) && !isHard(c.difficulty)).length;
  const getCardPoints = (diff?: QuestionDifficulty | string | number) => {
    if (isEasy(diff)) return 1;
    if (isHard(diff)) return 3;
    return 2;
  };

  const isCardMastered = (card: { id: string; state: FlashcardStateEnum }) => {
    if (card.state === FlashcardStateEnum.Mastered) return true;
    return deck.attempts.some((a) =>
      a.cardStates && Number(a.cardStates[card.id]) === FlashcardStateEnum.Mastered
    );
  };

  const masteredInDeck = deck.cards.filter(isCardMastered).length;
  const learningInDeck = deck.cards.filter((c) => !isCardMastered(c) && c.state === FlashcardStateEnum.Learning).length;

  const totalDeckMaxPoints = deck.cards.reduce((sum, c) => sum + getCardPoints(c.difficulty), 0);
  const totalDeckEarnedPoints = deck.cards.reduce(
    (sum, c) => (isCardMastered(c) ? sum + getCardPoints(c.difficulty) : sum),
    0
  );
  const masteredPct = totalDeckMaxPoints > 0 ? Math.round((totalDeckEarnedPoints / totalDeckMaxPoints) * 100) : deck.progressPercentage;

  return (
    <div className="max-w-4xl mx-auto space-y-6 animate-in fade-in duration-300">
      {/* Header & Actions */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3.5 min-w-0">
          <SecondaryButton
            type="button"
            onClick={handleGoBack}
            aria-label={t("flashcardDetails.backBtn")}
            icon={<ChevronLeft size={22} strokeWidth={2.25} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          />
          <div className="flex flex-col justify-center min-w-0">
            {deck.courseName && (
              <Badge variant="secondary" className="self-start mb-1">
                {deck.courseName}
              </Badge>
            )}
            <h1 className="text-xl font-bold text-foreground truncate">
              {deck.name}
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
            {t("flashcardDetails.editBtn")}
          </SecondaryButton>
          <SecondaryButton
            type="button"
            onClick={handleDeleteDeck}
            icon={<Trash2 size={15} strokeWidth={2.25} className="text-rose-400" />}
            className="py-2.5 px-3.5 text-xs font-semibold"
          >
            {t("flashcardDetails.deleteBtn")}
          </SecondaryButton>
          <PrimaryButton
            type="button"
            disabled={deck.cardCount === 0}
            onClick={handleStartStudyDirect}
            icon={<Play size={16} strokeWidth={2.25} />}
            className="py-2.5 px-4 text-xs font-semibold"
          >
            {t("flashcardDetails.startStudyBtn")}
          </PrimaryButton>
        </div>
      </div>

      {/* Unified Stats Card */}
      <Card className="p-4 sm:p-5">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3.5 xl:gap-4 divide-y sm:divide-y-0 sm:divide-x divide-border/50">
          {/* Questions/Cards Per Session */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-foreground dark:text-white shrink-0">
              <HelpCircle size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("flashcardDetails.stats.perAttempt")}
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {deck.cardCountPerAttempt ?? deck.cardCount}
              </span>
            </div>
          </div>

          {/* Card Bank Pool */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-foreground dark:text-white shrink-0">
              <Layers size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("flashcardDetails.stats.cardPool")}
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {deck.cardCount}
              </span>
            </div>
          </div>

          {/* Mastered Cards */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-emerald-600 dark:text-emerald-400 shrink-0">
              <CheckCircle2 size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("flashcardDetails.stats.mastered")}
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {masteredInDeck}
              </span>
            </div>
          </div>

          {/* Learning Cards */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-secondary/80 border border-border flex items-center justify-center text-amber-600 dark:text-amber-400 shrink-0">
              <BookOpen size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("flashcardDetails.stats.learning")}
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {learningInDeck}
              </span>
            </div>
          </div>
        </div>

        {/* Pool Mastery Progress Bar */}
        {deck.cardCount > 0 && (
          <div className="mt-4 pt-3.5 border-t border-border/50 space-y-2">
            <div className="flex items-center justify-between text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
              <div className="flex items-center gap-1.5">
                <span>{t("flashcardDetails.progress")}</span>
                <Tooltip content={t("flashcardDetails.difficultyWeightTooltip")}>
                  <div className="inline-flex items-center cursor-help text-muted-foreground/70 hover:text-foreground transition-colors">
                    <HelpCircle size={13} strokeWidth={2} />
                  </div>
                </Tooltip>
              </div>
              <span className="text-foreground font-bold">
                {t("flashcardDetails.progressText", { mastered: masteredInDeck, total: deck.cardCount, pct: masteredPct })}
              </span>
            </div>
            <MultiSegmentProgressBar
              segments={[
                {
                  id: "progres",
                  value: masteredInDeck,
                  colorClass: "bg-emerald-500",
                  customTooltip: t("flashcardDetails.tooltipMastered", { mastered: masteredInDeck, total: deck.cardCount, pct: masteredPct }),
                },
              ]}
              totalValue={deck.cardCount}
              heightClass="h-3.5"
            />
          </div>
        )}

        {/* Difficulty Distribution Bar */}
        {deck.cardCount > 0 && (
          <div className="mt-4 pt-3.5 border-t border-border/50 space-y-3">
            <div className="flex items-center justify-between text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
              <span>{t("flashcardDetails.difficulty")}</span>
            </div>

            <MultiSegmentProgressBar
              segments={[
                { id: "easy", value: easyInDeck, colorClass: "bg-emerald-500", customTooltip: `${t("quizSolver.difficulty.easy")} (${easyInDeck})` },
                { id: "medium", value: mediumInDeck, colorClass: "bg-amber-500", customTooltip: `${t("quizSolver.difficulty.medium")} (${mediumInDeck})` },
                { id: "hard", value: hardInDeck, colorClass: "bg-rose-500", customTooltip: `${t("quizSolver.difficulty.hard")} (${hardInDeck})` },
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
            {t("flashcardDetails.attemptHistory", { count: deck.attempts.length })}
          </h3>
        </div>

        {deck.attempts.length === 0 ? (
          <div className="bg-card rounded-xl border border-border p-10 text-center space-y-3">
            <Clock size={32} className="mx-auto text-muted-foreground/35 mb-2" />
            <p className="text-sm font-medium text-muted-foreground">{t("flashcardDetails.noAttempts")}</p>
            <p className="text-xs text-muted-foreground/60">{t("flashcardDetails.noAttemptsSubtitle")}</p>
            <div className="pt-2">
              <PrimaryButton
                disabled={deck.cardCount === 0}
                onClick={handleStartStudyDirect}
                icon={<Play size={15} strokeWidth={2.25} />}
                className="text-xs py-2 px-5"
              >
                {t("flashcardDetails.startStudyBtn")}
              </PrimaryButton>
            </div>
          </div>
        ) : (
          <div className="space-y-3">
            {[...deck.attempts]
              .sort((a, b) => new Date(a.startedAt).getTime() - new Date(b.startedAt).getTime())
              .map((attempt, index) => ({
                ...attempt,
                attemptNumber: index + 1
              }))
              .reverse()
              .map((attempt) => {
                const attemptNumber = attempt.attemptNumber;
                const isCompleted = attempt.status === QuizAttemptStatus.Completed;

                return (
                  <div
                    key={attempt.id}
                    onClick={() => handleOpenAttempt(attempt.id)}
                    className="group card-app-spring p-4 rounded-xl bg-card border border-border flex items-center justify-between gap-4 cursor-pointer select-none"
                  >
                    <div className="flex items-center gap-3.5 min-w-0">
                      <div className="w-9 h-9 rounded-lg bg-background border border-border flex items-center justify-center text-xs font-bold text-foreground shrink-0">
                        #{attemptNumber}
                      </div>

                      <div className="min-w-0">
                        <span className="text-xs font-bold text-foreground block tile-title-scale">
                          {t("flashcardDetails.attemptNum", { num: attemptNumber })}
                        </span>

                        <div className="text-[11px] text-muted-foreground mt-0.5 flex items-center gap-2 flex-wrap">
                          <span>{t("flashcardDetails.startedAt", { date: new Date(attempt.startedAt).toLocaleString(i18n.language) })}</span>
                          <span>•</span>
                          <span>{t("flashcardDetails.cardsCount", { count: attempt.cardCount })}</span>
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 shrink-0">
                      {isCompleted ? (
                        <div className="flex flex-col items-end justify-center text-right">
                          <span
                            className={`text-base md:text-lg font-bold tabular-nums leading-tight ${getScoreColorClass(attempt.progressPercentage)}`}
                          >
                            {attempt.progressPercentage}%
                          </span>
                          <span className="text-[11px] text-muted-foreground font-medium tabular-nums">
                            {attempt.masteredCount} / {attempt.cardCount} {t("flashcardDetails.cardsUnit")}
                          </span>
                        </div>
                      ) : (
                        <div className="flex flex-col items-end justify-center text-right">
                          <span className="text-xs font-bold text-amber-500 dark:text-amber-400 uppercase tracking-wider">
                            {t("quizDetails.inProgress", "W TOKU")}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                );
              })}
          </div>
        )}
      </div>

      <EditFlashcardDeckModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        deckId={deck.id}
        initialName={deck.name}
        initialEasyCount={deck.easyCardCountPerAttempt}
        initialMediumCount={deck.mediumCardCountPerAttempt}
        initialHardCount={deck.hardCardCountPerAttempt}
        easyInPool={easyInDeck}
        mediumInPool={mediumInDeck}
        hardInPool={hardInDeck}
        onSuccess={fetchDeckDetails}
      />
    </div>
  );
}
