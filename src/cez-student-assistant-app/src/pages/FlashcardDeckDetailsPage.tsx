import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { flashcardService } from "../services/flashcardService";
import type { FlashcardDeckDetailsDto } from "../types/flashcardTypes";
import { QuestionDifficulty, QuizAttemptStatus } from "../enums/quizEnums";
import Card from "../components/Card";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import { Alert } from "../components/Alert";
import LoadingScreen from "../components/LoadingScreen";
import { EditFlashcardDeckModal } from "../components/EditFlashcardDeckModal";
import { StartFlashcardStudyModal } from "../components/StartFlashcardStudyModal";
import MultiSegmentProgressBar from "../components/MultiSegmentProgressBar";
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
  const { t } = useTranslation();

  const [deck, setDeck] = useState<FlashcardDeckDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [isNavigatingBack, setIsNavigatingBack] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isStartStudyModalOpen, setIsStartStudyModalOpen] = useState(false);

  const fromPath = (location.state as { fromPath?: string })?.fromPath || (deck?.courseId ? `/course/${deck.courseId}` : "/courses");

  const fetchDeckDetails = async () => {
    if (!id) return;
    setLoading(true);
    setErrorMsg(null);
    try {
      const data = await flashcardService.getFlashcardDeckDetails(id);
      setDeck(data);
    } catch (err: any) {
      setErrorMsg(err.message || "Błąd podczas pobierania szczegółów zestawu fiszek.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDeckDetails();
  }, [id]);

  const handleGoBack = () => {
    setIsNavigatingBack(true);
    setTimeout(() => {
      if (location.state?.fromPath) {
        navigate(location.state.fromPath);
      } else if (deck?.courseId) {
        navigate(`/course/${deck.courseId}`);
      } else {
        navigate("/courses");
      }
    }, 200);
  };

  const handleDeleteDeck = async () => {
    if (!id || !deck) return;
    if (window.confirm("Czy na pewno chcesz usunąć ten zestaw fiszek?")) {
      try {
        await flashcardService.deleteFlashcardDeck(id);
        navigate(fromPath);
      } catch (err: any) {
        setErrorMsg(err.message || "Błąd podczas usuwania zestawu fiszek.");
      }
    }
  };

  const handleStartStudySession = async (easyCount: number, mediumCount: number, hardCount: number) => {
    if (!deck) return;
    const totalSelected = easyCount + mediumCount + hardCount;
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

  if (loading) {
    return <LoadingScreen message={t("flashcards.loadingDeckDetails")} />;
  }

  if (errorMsg || !deck) {
    return (
      <div className="flex-1 p-6 md:p-8 max-w-4xl mx-auto space-y-4">
        <button
          onClick={handleGoBack}
          className="flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
        >
          <ArrowLeft size={14} /> Powrót
        </button>
        <Alert variant="error" message={errorMsg || "Zestaw fiszek nie istnieje."} />
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
  const masteredPct = deck.cardCount > 0 ? Math.round((deck.masteredCardCount / deck.cardCount) * 100) : 0;

  return (
    <div
      className={`max-w-4xl mx-auto space-y-6 ${
        isNavigatingBack ? "animate-slide-out-right" : "animate-in fade-in duration-300"
      }`}
    >
      {/* Header & Actions */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3.5 min-w-0">
          <button
            type="button"
            onClick={handleGoBack}
            title="Powrót"
            className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors shadow-xs cursor-pointer shrink-0"
          >
            <ChevronLeft size={22} strokeWidth={2.25} className="shrink-0" />
          </button>
          <div className="flex flex-col justify-center min-w-0">
            {deck.courseName && (
              <span className="self-start px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20 mb-1">
                {deck.courseName}
              </span>
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
            Edytuj
          </SecondaryButton>
          <SecondaryButton
            type="button"
            onClick={handleDeleteDeck}
            icon={<Trash2 size={15} strokeWidth={2.25} className="text-rose-400" />}
            className="py-2.5 px-3.5 text-xs font-semibold"
          >
            Usuń
          </SecondaryButton>
          <PrimaryButton
            type="button"
            disabled={deck.cardCount === 0}
            onClick={() => setIsStartStudyModalOpen(true)}
            icon={<Play size={16} strokeWidth={2.25} />}
            className="py-2.5 px-4 text-xs font-semibold"
          >
            Rozpocznij naukę
          </PrimaryButton>
        </div>
      </div>

      {/* Unified Stats Card */}
      <Card className="p-4 sm:p-5">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3.5 xl:gap-4 divide-y sm:divide-y-0 sm:divide-x divide-border/50">
          {/* Questions/Cards Per Session */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0">
            <div className="w-9 h-9 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <HelpCircle size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                W PODEJŚCIU
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {deck.cardCountPerAttempt ?? deck.cardCount}
              </span>
            </div>
          </div>

          {/* Card Bank Pool */}
          <div className="flex items-center gap-2.5 pt-1 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
              <Layers size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                BAZA FISZEK
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {deck.cardCount}
              </span>
            </div>
          </div>

          {/* Mastered Cards */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400 shrink-0">
              <CheckCircle2 size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                UMIEM
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {deck.masteredCardCount}
              </span>
            </div>
          </div>

          {/* Learning Cards */}
          <div className="flex items-center gap-2.5 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-9 h-9 rounded-lg bg-amber-500/10 border border-amber-500/20 flex items-center justify-center text-amber-400 shrink-0">
              <BookOpen size={18} strokeWidth={2.25} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                UCZĘ SIĘ
              </span>
              <span className="text-lg font-bold text-foreground block truncate animate-in fade-in zoom-in-95 duration-300">
                {deck.learningCardCount}
              </span>
            </div>
          </div>
        </div>

        {/* Pool Mastery Progress Bar */}
        {deck.cardCount > 0 && (
          <div className="mt-4 pt-3.5 border-t border-border/50 space-y-2">
            <div className="flex items-center justify-between text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
              <span>PROGRES</span>
              <span className="text-foreground font-bold">
                {deck.masteredCardCount} / {deck.cardCount} FISZEK ({masteredPct}%)
              </span>
            </div>
            <MultiSegmentProgressBar
              segments={[
                {
                  id: "progres",
                  value: deck.masteredCardCount,
                  colorClass: "bg-emerald-500",
                  customTooltip: `Opanowane: ${deck.masteredCardCount} / ${deck.cardCount} (${masteredPct}%)`,
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
              <span>TRUDNOŚĆ</span>
            </div>

            <MultiSegmentProgressBar
              segments={[
                { id: "easy", value: easyInDeck, colorClass: "bg-emerald-500", customTooltip: `Łatwe (${easyInDeck})` },
                { id: "medium", value: mediumInDeck, colorClass: "bg-amber-500", customTooltip: `Średnie (${mediumInDeck})` },
                { id: "hard", value: hardInDeck, colorClass: "bg-rose-500", customTooltip: `Trudne (${hardInDeck})` },
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
            Historia podejść ({deck.attempts.length})
          </h3>
        </div>

        {deck.attempts.length === 0 ? (
          <div className="bg-card rounded-xl border border-border p-10 text-center space-y-3">
            <Clock size={32} className="mx-auto text-muted-foreground/35 mb-2" />
            <p className="text-sm font-medium text-muted-foreground">Brak wcześniejszych podejść do nauki tego zestawu.</p>
            <p className="text-xs text-muted-foreground/60">Rozpocznij naukę, aby przećwiczyć i utrwalić wiedzę!</p>
            <div className="pt-2">
              <PrimaryButton
                disabled={deck.cardCount === 0}
                onClick={() => setIsStartStudyModalOpen(true)}
                icon={<Play size={15} strokeWidth={2.25} />}
                className="text-xs py-2 px-5"
              >
                Rozpocznij naukę
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
                    className="p-4 rounded-xl bg-card border border-border flex items-center justify-between gap-4 transition-colors hover:border-primary/40 hover:bg-muted/30"
                  >
                    <div className="flex items-center gap-3.5 min-w-0">
                      <div className="w-9 h-9 rounded-lg bg-background border border-border flex items-center justify-center text-xs font-bold text-foreground shrink-0">
                        #{attemptNumber}
                      </div>

                      <div className="min-w-0">
                        <span className="text-xs font-bold text-foreground block">
                          Podejście #{attemptNumber}
                        </span>

                        <div className="text-[11px] text-muted-foreground mt-0.5 flex items-center gap-2 flex-wrap">
                          <span>Rozpoczęto: {new Date(attempt.startedAt).toLocaleString()}</span>
                          <span>•</span>
                          <span>{attempt.cardCount} fiszek</span>
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
                            {attempt.masteredCount} opanowanych / {attempt.learningCount} w trakcie
                          </span>
                        </div>
                      ) : (
                        <div className="flex flex-col items-end justify-center text-right">
                          <span className="text-base md:text-lg font-bold text-amber-500 dark:text-amber-400 leading-tight">
                            W toku
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

      <StartFlashcardStudyModal
        isOpen={isStartStudyModalOpen}
        onClose={() => setIsStartStudyModalOpen(false)}
        onStart={handleStartStudySession}
        cards={deck.cards}
      />
    </div>
  );
}
